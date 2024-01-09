using Godot;
using System;


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
1. 	inspection(Lines/Texture) 		- текстура закрывает экран с подписью
									снизу (прм. могила Кловер). Повторяется
									при каждом взаимодействии.
2. 	initialIInteractinLines 		- текст самого первого взаимодействия с
									объектом, не повторяется
3. 	interactionLines 				- текст второго и каждого последующего
									взаимодействия с объектом.
4.	autocast 						- автоматическое применение заклинания
									(если задано) при каждом взаимодействии
5.	castForwardIntercationLines		- текст отображаемый сразу после применения
									заклинания.
6.	
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
	[Export] public string[] InitialInteracitonLines;

	[ExportGroup("Interaction")]
	[Export] public string[] InteractionLines;

	[ExportGroup("Spell")]
	/* Строковое название заклинания, заменяется
	соответствующим кодом из словаря в классе
	GlobalVariables, если существует иначе
	пустая строка. */ 
	[Export] string spellName = "";
	/* Заклинание применяется автоматически,
	ака	при изучении нового заклинания. */
	[Export] public bool Autocast = false;
	/* Текст после прямого применения заклинания
	(F stands for Forward). */
	[Export] public string[] FCastIntercationLines;
	/* Текст после обратного применения заклинания
	(B stands for Взад (ну или Backward)). */
	[Export] public string[] BCastInteractionLines;
	/* Реакция на попытку применить уже примененного
	заклинания в данном направлении (а-ля "Колба уже
	левитирует"). */
	[Export] public string[] ErrFCastInteractionLines;
	[Export] public string[] ErrBCastInteractionLines;
	
	private bool isReached = false;
	
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
			GetTree().CallGroup("UI", "InteractableUnhovered");
		clickable.InputEvent += (viewport, @event, shapeIdx) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
				{
					(viewport as Viewport).SetInputAsHandled();
					OnClick();
				}
		};
    }

	public virtual void OnClick()
	{
		var tree = GetTree();
		if (!isReached)
			tree.CallGroup("Player", "InteractableClicked", this);
		else
		{
			if (InspectionTexture != null)
				tree.CallGroup("UI", "InteractableInspected", this);
			else if (!InitialInteracitonLines.IsEmpty())
				tree.CallGroup("UI", "InteractableInitialInteraction", this);
			else if (!InteractionLines.IsEmpty())
				tree.CallGroup("UI", "InteractableInteracted", this);
			else if (Autocast)
				tree.CallGroup("UI", "InteractableSpellReveal", GlobalVariables.GetSpellCode(spellName));
		}
	}

	/* =================== Вызываются напрямую из UI ===================  */

	public virtual void OnInspectionFinished()
	{
		if (!InitialInteracitonLines.IsEmpty())
			GetTree().CallGroup("UI", "InteractableInitialInteraciton", this);
		else if (!InteractionLines.IsEmpty())
			GetTree().CallGroup("UI", "InteractableInteracted", this);
	}

	public virtual void OnInteractionFinished()
	{
		InitialInteracitonLines = Array.Empty<string>();
	}


	/* Вызываются через GetTree().CallGroup("Interactables", ...) */

	protected virtual void InteractableReached(Interactable i)
	{
		if (this == i) isReached = true;
	}

	protected virtual void InteractableLeft(Interactable i)
	{
		if (this == i) isReached = false;
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

	protected virtual void CastOnInteractable(Interactable i, string spellCode)
	{
		if (this != i) return;
		if (GlobalVariables.GetSpellCode(spellName) == spellCode)
			GD.Print($"{spellName.ToUpper()} as {spellCode} is cast on {UILabel}");
	}
}
