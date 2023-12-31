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
	private bool _isActive = true;
	[Export] public bool IsActive {
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


	private void Disable() => IsActive = false;
	private void Enable() => IsActive = true;

	private int counter = 0;

    public override void _Ready()
    {
		var collider = GetNode<Area2D>("Collider");
		collider.AreaEntered += (area) =>
		{
			if (!area.IsInGroup("PlayerCollider")) return;
			counter++;
			GD.Print($"Collided {counter}");
		};
    }
}
