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
	public bool IsActive = false;


	private void MakeInactive()
	{
		IsActive = false;
	}


    private void OnClickableInputEvent(Node viewport, InputEvent @event, int shapeIdx)
	{
		IsActive = true;
	}
}
