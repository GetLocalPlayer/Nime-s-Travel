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
	[Export] public CompressedTexture2D UIIcon;
	[Export] public string UILabel;
	[Export] public string[] Interaction;
	
	public Vector2 WayPoint { get => GetNode<Marker2D>("WayPoint").GlobalPosition ;}
	public Vector2 LookAtPoint { get => GetNode<Marker2D>("LookAtPoint").GlobalPosition ;}

    public override void _Ready()
    {
		var clickable = GetNode<Area2D>("Clickable");
		clickable.MouseEntered += () =>
			GetTree().CallGroup("UI", "InteractableHovered", this);
		clickable.MouseExited += () =>
			GetTree().CallGroup("UI", "InteractableUnhovered", this);
		clickable.InputEvent += (viewport, @event, shapeIdx) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
				{
					((Viewport)viewport).SetInputAsHandled();
					GetTree().CallGroup("Player", "InteractableClicked", this);
					GetTree().CallGroup("UI", "InteractableClicked", this);
				}
		};
    }
}
