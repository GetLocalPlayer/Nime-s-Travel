using Godot;
using System;

public partial class Background : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var clickable = GetNode<Area2D>("Clickable");
		
		clickable.InputEvent += (Nodeviewport, @event, shapeIdx) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
					GetTree().CallGroup("Player", "SetNewWalkTarget", GetGlobalMousePosition());
		};
	}
}
