using Godot;
using System;
using System.Dynamic;


/*
Данная сцена описывает базовый объект с которым может
взаимодействовать Ниме.
-Корень
--Sprite2D		// Визуал.
--Clickable		// Нода типа Area2D для отлова клика мышкой.
--Collider		// Нода типа Area2D для отлова столкновения с Нимэ,
				// фактически - область, в которой должна находиться
				// Нимэ чтобы взаимодействие произошло.

На основе этой сцены создаются прочие объекты взаимодейтсвия.
*/

public partial class Interactable : Node2D
{
	private bool _isActive;
	public bool IsActive {
		get => _isActive;
		set {
			_isActive = value;
			if (IsNodeReady())
			{
				var collider = GetNode<Area2D>("Collider");
				collider.Monitoring = value;
				collider.Visible = value;
			}
		}
	}
	[Export] public CompressedTexture2D Icon;
	[Export] public string UIName;
	[Export] public string[] UIInteraction;
	private bool isHovered = false;

	public void Disable() => IsActive = false;
	public void Enable() => IsActive = true;


    public override void _Ready()
    {
		Disable();

		var collider = GetNode<Area2D>("Collider");
		collider.AreaEntered += (area) =>
		{
			if (!area.IsInGroup("PlayerCollider")) return;
			SetDeferred("IsActive", false);
		};

		var clickable = GetNode<Area2D>("Clickable");
		clickable.MouseEntered += () =>
		{
			isHovered = true;
			GetTree().CallGroup("UI", "SetInteractable", this);
		};
		clickable.MouseExited += () =>
		{
			isHovered = false;
			GetTree().CallGroup("UI", "RemoveInteractable", this);
		};
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton btn)
			if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
				IsActive = isHovered;
    }
}
