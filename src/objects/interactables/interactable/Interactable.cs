using Godot;
using System;
using System.Collections.Generic;
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
	/* Иконка отображаемая при взаимодействии в 
	игре (правый-нижний угол). */
	[Export] public CompressedTexture2D UIIcon;
	/* Название объекта под иконкой. */
	[Export] public string UILabel;
	/* InitInteraction содержит текст при первом
	взаимодействии, после чего поле обнуляется
	и всегда возвращается Interaciton (см.
	GetInteraction). */
	[Export] private string[] initialInteracitonLines;
	[Export] private string[] interactionLines;
	/* inspection* используятся для более близкого
	взаимодействия с объектом (пр. могила Кловер).
	Всегда происходит перед initialInteraction и
	interaction. */
	[Export] private CompressedTexture2D inspectionTexture;
	[Export] private string[] inspecitonLines;
	
	public Vector2 WayPoint {
		get => GetNode<Marker2D>("WayPoint").GlobalPosition;
	}
	public Vector2 LookAtPoint {
		get => GetNode<Marker2D>("LookAtPoint").GlobalPosition;
	}

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
					OnClicked();					
		};
    }


	protected virtual void OnClicked()
	{
		GetViewport().SetInputAsHandled();
		/* ВАЖНО сначала вызвать функцию InteractableClicked 
		в группе игрока "Player" и только после этого в группе
		интерфейса "UI". Именно такая последовательность важна
		потому что я уже не помню почему, я написал этот 
		комментарий через неделю после кода и уже хз почему
		оно так. */
		GetTree().CallGroup("Player", "InteractableClicked", this);
		GetTree().CallGroup("UI", "InteractableClicked", this);
	}


	public (CompressedTexture2D, string[]) GetInspectionData() => 
		(inspectionTexture, inspecitonLines);

	public string[] GetInteractionLines()
	{
		string[] lines = null;
		if (!initialInteracitonLines.IsEmpty())
		{
			lines = (string[])initialInteracitonLines.Clone();
			initialInteracitonLines = null;
		}
		else if (!interactionLines.IsEmpty())
		{
			lines = (string[])interactionLines.Clone();
		}
		return lines;
	}
}
