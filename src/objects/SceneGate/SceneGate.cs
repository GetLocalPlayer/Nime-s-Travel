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
		Я удаляю лишнюю копию Ниме из загруженной
		сцены (в каждой сцене есть своя копия Ниме
		для тестов).
		Экземпляр SceneGate хранят в поле storedScene
		ссылку на сцену, на которую обеспечивают
		переход. */
		
		/* Древо следует иметь в переменной, поскольку
		после удаления текущей сцены, вызов метода
		GetTree будет возвращать null. */
		var tree = GetTree();

		/* Прячем текущую сцену (RemoveChild не
		удаляет нод из памяти, а просто отсоединяет
		его, скрывает, останавливает процессы). */
		var removedScene = GetTree().CurrentScene;
		tree.Root.RemoveChild(removedScene);
		// Достаем Ниме
		var nime = removedScene.GetNode<Nime>("Nime");
		removedScene.RemoveChild(nime);
		/* Загружаем новую сцену по указанному пути
		в поле ToScenPath если ранее не была загружена. */
		if (storedScene == null)
		{
			var packedScene = ResourceLoader.Load<PackedScene>(ToScenePath);
			storedScene = packedScene.Instantiate();
		}
		/* Удаляем копию Ниме в загруженной сцене
		(для тестов в каждую сцену была добавлена
		своя Ниме). */
		if (storedScene.HasNode("Nime"))
		{
			var n = storedScene.GetNode<Nime>("Nime");
			storedScene.RemoveChild(n);
			nime.MoveSpeed = n.MoveSpeed;
			n.QueueFree();
		}
		/* Устанавливаем текущую сцену. */
		tree.Root.AddChild(storedScene);
		tree.CurrentScene = storedScene;
		/* Передаем воротам в новой сцене ссылку
		на текущую, чтобы не загружать ее дважды
		при возврате. Ворота между двумя сценами
		дожны иметь одинаковое имя в обеих сценах. */
		var newGate = storedScene.GetNode<SceneGate>(new NodePath("%" + Name));
		newGate.storedScene = removedScene;
		/* Переносим Ниме в новую сцену но на всякий
		пожарный через deferred вызов, когда все функции
		движка в текущий кадр выполнены  (P.S. API
		движка написано в snake_case, поэтому передается
		имя оригинальной функции в snake_case, а не
		в C# обретке CamelCase). */
		nime.Disable();
		storedScene.CallDeferred("add_child", nime);
		tree.CallDeferred("call_group", "Player", "EnterScene", newGate);
		nime.CallDeferred("Enable");
	}
}
