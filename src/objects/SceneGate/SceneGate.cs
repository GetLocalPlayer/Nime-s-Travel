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
	[Export] public string ToScenePath;
	private PackedScene ToScene;
	private bool _isActive = true;
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
		/* CallDeferred вызывает функцию с указанным
		в строке именем в конце текущего кадра. Это
		хорошее место чтобы отложить вызов функций
		для которых требуется чтобы все ноды сцены были
		инициализированны, готовы к работе и все
		внутренние процессы движка звершены. */
		CallDeferred("ReadyDeferred");

		ToScene = ResourceLoader.Load<PackedScene>(ToScenePath);

		GetNode<Area2D>("Clickable").Monitoring = IsActive;
		GetNode<Area2D>("Clickable").InputPickable = IsActive;
		GetNode<Area2D>("Collider").Monitoring = IsActive;	

        var clickable = GetNode<Area2D>("Clickable");
		clickable.InputEvent += (viewport, @event, shapeIdx) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
				{
					GetTree().CallGroup("Player", "SetNewWalkTarget", GetNode<Marker2D>("ExitWayPoint").GlobalPosition);
					((Viewport)viewport).SetInputAsHandled();
				}
		};

		var collider = GetNode<Area2D>("Collider");
		collider.AreaEntered += (area) =>
		{
			if (area.IsInGroup("PlayerCollider"))
				/* Выгружать текущую сцену в момент
				выполнения кода сигнала - не лучшая идея,
				поскольку движок продолжет выполнение
				внутренних процессов связанных с нодой
				текущего сигнала, что приведет к
				исключению нулевого указателя.
				Выгружать сцену следует в конце текущего 
				кадра, когда все внутренние процессы завершены. */
				CallDeferred("ChangeScene");
		};
    }

	private void ReadyDeferred()
	{
		var globals = GetTree().Root.GetNode<GlobalVariables>("GlobalVariables");

		if (Name == globals.SceneGateName)
			GetTree().CallGroup("Player", "EnterScene", GetNode<Marker2D>("SpawnPoint").GlobalPosition, GetNode<Marker2D>("SpawnWayPoint").GlobalPosition);
	}

	private void ChangeScene()
	{
		var globals = GetTree().Root.GetNode<GlobalVariables>("GlobalVariables");
		globals.SceneGateName = Name;
		GetTree().ChangeSceneToPacked(ToScene);
	}
}
