using Godot;
using System;


/*
Нода для перехода между сценами.

 	SceneGate			// Просто корневая нода для группировки
 	--SpawnPoint		// Координаты этой ноды передаются Нимэ сразу после загрузки сцены
	--SpawnWayPoint		// Точка координат для NavigationAgent2D, к которой будет проложен
						// путь сразу после загрузки сцены (из SpawnPoint в SpawnWayPoint)
 	--ExitWayPoint		// Точка координат для NavigationAgent2D, к которой будет проложен
						// путь для выхода из сцены при клике на Clickable
	--Collider			// С этой нодой должна столкнуться Нимэ чтобы начался процесс
						// загрузки уровня, соответствующему данному переоду
	--Clicable			// Провоцирует Нимэ проложить путь к ExitPoint, на пути к которому
						// она должна столкнутьсяс Collider и активировать переход на
						// соответствующую сцену.
*/
public partial class SceneGate : Node2D
{
	/* Путь к сцене на которую обеспечиавют
	переход данные ворота. */
	[Export] public string ToScenePath;
	Node storedScene;

	bool _isActive = true;
	[Export] public bool IsActive {
		get => _isActive;
		set {
			_isActive = value;
			// На момент первого вызова этого сеттера,
			// нод и его потомки еще не готовы и попытка
			// обращения вызовает исключение null указателя.
			if (IsNodeReady())
			{
				GetNode<Area2D>("Clickable").Monitoring = value;
				GetNode<Area2D>("Clickable").InputPickable = value;
				GetNode<Area2D>("Collider").Monitoring = value;
			}
		}
	}
	public Vector2 SpawnPoint { get => GetNode<Marker2D>("SpawnPoint").GlobalPosition; }
	public Vector2 SpawnWayPoint { get => GetNode<Marker2D>("SpawnWayPoint").GlobalPosition; }
	public Vector2 ExitWayPoint { get => GetNode<Marker2D>("ExitWayPoint").GlobalPosition; }

	// Для активации/деактивации посредстом вызова группы
	// GetTree().CallGroup("SceneGates", "Disable"/"Enable")
	public void Disable() => IsActive = false;
	public void Enable() => IsActive = true;

    public override void _Ready()
    {
		GetNode<Area2D>("Clickable").Monitoring = IsActive;
		GetNode<Area2D>("Clickable").InputPickable = IsActive;
		GetNode<Area2D>("Collider").Monitoring = IsActive;	

        var clickable = GetNode<Area2D>("Clickable");
		clickable.InputEvent += (viewport, @event, shapeIdx) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
				{
					GetTree().CallGroup("Player", "SceneGateClicked", this);
					((Viewport)viewport).SetInputAsHandled();
				}
		};

		var collider = GetNode<Area2D>("Collider");
		collider.AreaEntered += (area) =>
		{
			if (area.IsInGroup("PlayerCollider"))
				/* Смена текущей сцены в момент
				выполнения кода сигнала - не лучшая идея,
				поскольку движок продолжет выполнение
				внутренних процессов связанных с нодой
				текущего сигнала, что приведет к
				исключению нулевого указателя.
				Менять сцену следует в конце текущего 
				кадра, когда все внутренние процессы завершены. */
				CallDeferred("ChangeScene");
		};
    }

	private void ChangeScene()
	{
		/* Я не удаляю текущую сцену из памяти при
		переходе в новую локацию, я отсоединяю ее 
		от древа, перенося Ниме из одной сцены в 
		другую.
		Экземпляры SceneGate хранят в поле storedScene
		ссылку на сцену, на которую обеспечивают
		переход. */
		
		/* Древо следует поместить в переменную,
		поскольку, после удаления текущей сцены
		из которой работает текущий SceneGate,
		вызов метода GetTree будет возвращать null. */
		var tree = GetTree();

		/* Прячем текущую сцену (RemoveChild не
		удаляет нод из памяти, а просто отсоединяет
		его, скрывает, останавливает процессы). */
		var removedScene = GetTree().CurrentScene;
		tree.Root.RemoveChild(removedScene);
		// Достаем Ниме из текущей сцены
		var nime = removedScene.GetNode<Nime>("Nime");

		/* В процессе тестов, в каждую сцену была добавлена
		своя копия Ниме. Вместо удаления данных копий я решил
		их оставить, чтобы брать от них параметры оптимально
		подобранные в процессе тестов (пока что это только
		MoveSpeed). Копию я просто переименовываю в NimeParams. */

		/* Если в удаляемой сцене нет нода NimeParams, значит
		эта сцена была загружена при запуске игры и нужно
		создать копию Ниме. */
		if (!removedScene.HasNode("NimeParams"))
		{
			var copy = nime.Duplicate((int)DuplicateFlags.Scripts) as Nime;
			copy.Disable();
			copy.Hide();
			copy.Name = "NimeParams";
			foreach (var child in copy.GetChildren())
			{
				copy.RemoveChild(child);
				child.QueueFree();
			}
			removedScene.AddChild(copy);
		}
		removedScene.RemoveChild(nime);
		/* Загружаем новую сцену по указанному пути
		в поле ToScenPath если ранее не была загружена. */
		var loadedScene = storedScene ?? ResourceLoader.Load<PackedScene>(ToScenePath).Instantiate();
		storedScene = loadedScene;
		/* Чищу копию Ниме перед сменой сцены. */
		if (!loadedScene.HasNode("NimeParams"))
		{
			var n = loadedScene.GetNode<Nime>("Nime");
			n.Disable();
			n.Hide();
			n.Name = "NimeParams";
			foreach (var child in n.GetChildren())
			{
				n.RemoveChild(child);
				child.QueueFree();
			}
			foreach (var group in n.GetGroups())
				n.RemoveFromGroup(group);
		}
		nime.MoveSpeed = loadedScene.GetNode<Nime>("NimeParams").MoveSpeed;
		/* Устанавливаем текущую сцену. */
		tree.Root.AddChild(loadedScene);
		tree.CurrentScene = loadedScene;
		/* Передаем воротам в новой сцене ссылку
		на текущую, чтобы не загружать ее дважды
		при возврате. Ворота между двумя сценами
		дожны иметь одинаковое имя в обеих сценах. */
		var loadedGate = loadedScene.GetNode<SceneGate>(new NodePath("%" + Name));
		loadedGate.storedScene = removedScene;
		/* Переносим Ниме в новую сцену но на всякий
		пожарный через deferred вызов, когда все функции
		движка в текущий кадр выполнены  (P.S. API
		движка написано в snake_case, поэтому передается
		имя оригинальной функции в snake_case, а не
		в C# обертке CamelCase). */
		loadedScene.CallDeferred("add_child", nime);
		tree.CallDeferred("call_group", "Player", "EnterScene", loadedGate);
	}
}
