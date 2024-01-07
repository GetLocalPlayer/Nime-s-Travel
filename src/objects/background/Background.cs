using Godot;
using System;

public partial class Background : Sprite2D
{
	/* Задник отлавливает клик мышкой и передает
	группе игроков глобальную позицию курсора
	к которой строится маршрут по навигационной
	сетке. */
	public override void _Ready()
	{
		var clickable = GetNode<Area2D>("Clickable");
		
		clickable.InputEvent += (Nodeviewport, @event, shapeIdx) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
					GetTree().CallGroup("Player", "BackgroundClicked", GetGlobalMousePosition());
		};
	}
}
