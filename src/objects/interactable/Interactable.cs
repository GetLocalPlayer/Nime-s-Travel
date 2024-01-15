using Godot;
using System;
using System.Linq;


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

Последовательность взаимодействия:
1. 	Inspection(Lines/Texture) 		- текстура закрывает экран с подписью
									снизу (прим. могила Кловер). Повторяется
									при каждом взаимодействии.
2. 	FirstIInteractinLines 			- текст самого первого взаимодействия с
									объектом, не повторяется, следует после
									Inspection.
3. 	InteractionLines 				- текст второго и каждого последующего
									взаимодействия с объектом.
4.	Autocast 						- автоматическое применение заклинания
									(если задано) при каждом взаимодействии
5.	SpellInteractionLines			- текст отображаемый сразу после применения
									заклинания.
6.	WrongSpellInteracitonLines		- текст отображаемый в случае если использованное
									заклинание не принимается.
*/


public partial class Interactable : Node2D
{
	[ExportGroup("UI")]
	/* Иконка отображаемая при взаимодействии в 
	игре (правый-нижний угол). */
	[Export] public CompressedTexture2D UIIcon;
	/* Название объекта под иконкой. */
	[Export] public string UILabel;
	/* inspection* используятся для более близкого
	взаимодействия с объектом (пр. могила Кловер).
	Всегда происходит перед initialInteraction и
	interaction. */
	[ExportGroup("Inspection")]
	[Export] public CompressedTexture2D InspectionTexture;
	[Export] public string[] InspectionLines;

	[ExportGroup("First Interaction")]
	/* InitInteraction содержит текст при первом
	взаимодействии, после чего поле чистится
	и всегда возвращается Interaciton (см.
	GetInteraction). */
	[Export] public string[] FirstInteracitonLines;

	[ExportGroup("Interaction")]
	[Export] public string[] InteractionLines;

	[ExportGroup("Applied Spell")]
	/* Имя заклинания которое можно использовать
	на данный Interactable.  */ 
	[Export] public string SpellName;
	/* Заклинание применяется автоматически,
	как	при изучении нового заклинания (прим.
	колба левитации). */
	[Export] public bool Autocast = false;
	/* Текст после применения заклинания. */
	[Export] public string[] SpellInteractionLines;
	/* Текст неверного заклинания. */
	[Export] public string[] WrongSpellInteractionLines;
	
	/* Ссылка на нод интерфейса чтобы не
	повторять GetTree().Root.GetNode... */
	protected UI ui;
	/* Флаг для запуска взаимодействия первой встречи. */
	protected bool met = false;
	/* Флаг сигнализирующий что Ниме дошла до точке
	взаимодействия с текущим Interactable по клику
	на Clickable и может с ним взаимодействовать. */
	bool isReached = false;
	
	public Vector2 WayPoint {
		get => GetNode<Marker2D>("WayPoint").GlobalPosition;
	}
	public Vector2 LookAtPoint {
		get => GetNode<Marker2D>("LookAtPoint").GlobalPosition;
	}

    public override void _Ready()
    {
		ui = GetTree().Root.GetNode<UI>("UI");

		var clickable = GetNode<Area2D>("Clickable");

		clickable.MouseEntered += () =>
			ui.SetHint(UIIcon, UILabel);

		clickable.MouseExited += () =>
			ui.ClearHint();

		clickable.InputEvent += (viewport, @event, shapeIdx) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
				{
					(viewport as Viewport).SetInputAsHandled();
					Clickable_OnClick();
				}
		};
    }

	async public virtual void Clickable_OnClick()
	{
		var tree = GetTree();
		if (!isReached)
			tree.CallGroup("Player", "InteractableClicked", this);
		else
		{
			if (InspectionTexture != null)
			{
				ui.RunPicturedInteraction(InspectionTexture, InspectionLines);
				await ToSignal(ui, "InteractionFinished");
				OnInspectionFinished();
			}
			if (!met && !FirstInteracitonLines.IsEmpty())
			{
				met = true;
				ui.RunInteraction(FirstInteracitonLines);
				await ToSignal(ui, "InteractionFinished");
				OnFirstInteractionFinished();
			}
			else if (!InteractionLines.IsEmpty())
			{
				ui.RunInteraction(InteractionLines);
				await ToSignal(ui, "InteractionFinished");
				OnInteractionFinished();
			}
			if (Autocast && SpellName != "" && SpellName != null)
			{
				ui.RevealSpell(SpellName);
				await ToSignal(ui, "SpellRevealed");
				tree.CallGroup("Player", "InteractableSpellRevealed", SpellName);
				OnSpellRevealed();
			}
		}
	}

	protected virtual void OnFirstInteractionFinished()
	{
		GD.Print("First interaction finished");
	}

	protected virtual void OnInspectionFinished()
	{
		GD.Print("Inspection finished");
	}

	protected virtual void OnInteractionFinished()
	{
		GD.Print("Interaction finished");
	}

	protected virtual void OnSpellRevealed()
	{
		GD.Print("Spell revealed");
		GetTree().CallGroup("Player", "InteractableSpellRevealed", SpellName);
		OnSpellCast(SpellName);
	}

	protected virtual void OnSpellCast(string spellName)
	{
		GD.Print("Spell is cast");
		ui.RunInteraction(spellName == SpellName ? SpellInteractionLines : WrongSpellInteractionLines);
	}

	/* Функции ниже вызываются через GetTree().CallGroup("Interactables", ...) */

	protected virtual void InteractableReached(Interactable i)
	{
		if (this != i) return;
		isReached = true;
		ui.SetHintButton(UIIcon, UILabel);
		ui.ShowHorn();
		ui.InteractableButtonClicked += Clickable_OnClick;
	}

	protected virtual void InteractableLeft(Interactable i)
	{
		if (this != i) return;
		isReached = false;
		ui.ClearHintButton();
		ui.HideHorn();
		ui.InteractableButtonClicked -= Clickable_OnClick;
	}

	protected virtual void StartCastOnInteractable(Interactable i)
	{
		if (this != i) return;
		GetNode<MagicEffect>("MagicEffect").Appear();
	}

	protected virtual void StopCastOnInteractable(Interactable i)
	{
		if (this != i) return;
		GetNode<MagicEffect>("MagicEffect").Disappear();
	}

	protected virtual void CastOnInteractable(Interactable i, string spellName)
	{
		if (this != i) return;

		OnSpellCast(spellName);
	}
}
