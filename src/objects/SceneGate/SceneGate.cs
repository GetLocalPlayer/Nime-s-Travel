using Godot;
using System;


/*
Нода для перехода между сценами.

 	SceneGate			// Просто корневая нода для группировки
 	--SpawnPoint		// Координаты этой ноды передаются Нимэ сразу после загрузки сцены
	--WalkInPoint		// Точка координат для NavigationAgent2D, к которой будет проложен
						// путь сразу после загрузки сцены (из SpawnPoint в WalkInPoint)
 	--WalkOutPoint		// Точка координат для NavigationAgent2D, к которой будет проложен
						// путь для выхода из сцены при клике на Clickable
	--Collider			// С этой нодой должна столкнуться Нимэ чтобы начался процесс
						// загрузки уровня, соответствующему данному переоду
	--Clicable			// Провоцирует Нимэ проложить путь к ExitPoint, на пути к которому
						// она должна столкнутьсяс Collider и активировать переход на
						// соответствующую сцену.
*/
public partial class SceneGate : Node2D
{
	[Export] public PackedScene ToScene;
	private bool isActive = true;
	[Export] public bool IsActive {
		get => isActive;
		set {
			isActive = value;
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
	public Vector2 WalkInPoint { get => GetNode<Marker2D>("WalkInPoint").GlobalPosition; }
	public Vector2 WalkOutPoint { get => GetNode<Marker2D>("WalkOutPoint").GlobalPosition; }

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
					GetTree().CallGroup("Player", "SetNewWalkTarget", GetNode<Marker2D>("WalkOutPoint").GlobalPosition);
					((Viewport)viewport).SetInputAsHandled();
				}
		};

		var collider = GetNode<Area2D>("Collider");
		collider.AreaEntered += (area) =>
		{
			if (area.IsInGroup("PlayerCollider"))
				// Судя по всему, смена сцены, как и другие 
				// действия связанные с удалением нодов,
				// вызывает проблемы, если заниматься этим
				// в момент обработки ряда сигналов.
				// CallDeferred вызывает функцию с указаным
				// именем после выполнения всех обработок
				// движком в текущем кадре.
				CallDeferred("ChangeScene");
		};
    }

	private void ChangeScene() => GetTree().ChangeSceneToPacked(ToScene);
}
