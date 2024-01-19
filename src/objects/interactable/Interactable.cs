using Godot;
using System;
using System.Linq;


/*
Interactable is a base game object Nime can interract
with. The main structure of the scene is following:

Root
|--Object					// Visual sprite of the object
|--MagicEffect				// Visual sprite for magic "cloud" effect during spell cast
|	|-- AnimationPlayer 	// Animations of the MagicEffect
|	|-- AnimationTree		// Transitions and blendings for animations above
|--	Clickable				// Area2D to catch mouse clicks to interract with the object
|	|-- CollisionShape2D	// Collision shape required by Area2D
|-- LookAtPoint				// The point Nime will be looking at
|-- WayPoint				// The position Nime must reach to interract with the object


Interaction order is next (each action runs one after
another unless it was changed in an inheriting class).

1. 	Inspection(Lines/Texture) 		- a texture covers the screen with specific
									text (check Clover's Grave interaction).
2. 	FirstInteractinLines 			- the text shown on very first interaction.
									Exported field 'Met' must be set to 'false',
									after the very first interaction 'Met' field
									is set to 'true'.
3. 	InteractionLines 				- the text shown on each next interaction after
									the very first one (if 'Met' field is set to
									'true' in other words).
4.	Autocast 						- makes the spell set in 'SpellName' field
									be cast automatically right after interaction.
									This makes Nime learn the spell.
									(если задано) при каждом взаимодействии.
5.	SpellInteractionLines			- the text shown if Nime has cast the same spell
									as the one set in 'SpellName' field.
6.	WrongSpellInteracitonLines		- the text shown if Nime has cast a spell that
									differs from the one set in 'SpellName' field.
*/


public partial class Interactable : Node2D
{
	[ExportGroup("UI")]
	/* Icon/Label shown in UI's left-right corner. */
	[Export] public CompressedTexture2D UIIcon;
	[Export] public string UILabel;	

	[ExportGroup("Inspection")]
	[Export] public CompressedTexture2D InspectionTexture;
	[Export] public string[] InspectionLines;

	[ExportGroup("First Interaction")]
	[Export] public bool Met = false;
	[Export] public string[] FirstInteracitonLines;

	[ExportGroup("Interaction")]
	[Export] public string[] InteractionLines;

	[ExportGroup("Applied Spell")]
	[Export] public string SpellName = "";
	[Export] public bool Autocast = false;
	[Export] public string[] SpellInteractionLines;
	[Export] public string[] WrongSpellInteractionLines;

	protected UI ui;
	/* A flag that tells that Nime reached the current
	interactable and can interract/cast spells. */
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

		var cursorSetup = GetTree().Root.GetNode<CursorSetup>("CursorSetup");

		clickable.MouseEntered += () =>
		{
			ui.SetHint(UIIcon, UILabel);
			cursorSetup.SetCursorIcon(CursorSetup.Shape.Wtf);
		};

		clickable.MouseExited += () =>
		{
			ui.ClearHint();
			cursorSetup.SetCursorIcon(CursorSetup.Shape.Default);
		};

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

	public virtual void Clickable_OnClick()
	{
		var tree = GetTree();
		if (!isReached)
			tree.CallGroup("Player", "InteractableClicked", this);
		else
		{
			OnInspection();
		}
	}

	/* Functions of basic interaction logic. Overridden in 
	inheritors. */

	async protected virtual void OnInspection()
	{
		GD.Print($"{Name} - inspection");
		if (InspectionTexture != null)
		{
			ui.RunPicturedInteraction(InspectionTexture, InspectionLines);
			await ToSignal(ui, "InteractionFinished");
		}
		OnInspectionFinished();
	}

	protected virtual void OnInspectionFinished()
	{
		GD.Print($"{Name} - inspection finished");
		if (!Met)
			OnFirstInteraction();
		else
			OnInteraction();
	}

	async protected virtual void OnFirstInteraction()
	{
		GD.Print($"{Name} - first interaction");
		if (!FirstInteracitonLines.IsEmpty())
		{
			ui.RunInteraction(FirstInteracitonLines);
			await ToSignal(ui, "InteractionFinished");
		}
		Met = true;
		OnFirstInteractionFinished();
	}

	protected virtual void OnFirstInteractionFinished()
	{
		GD.Print($"{Name} - first interaction finished");
		if (Autocast)
			OnSpellReveal();
	}

	protected virtual void OnInteraction()
	{
		GD.Print($"{Name} - interaction");
		if (!InteractionLines.IsEmpty())
			ui.RunInteraction(InteractionLines);
		OnInteractionFinished();
	}

	protected virtual void OnInteractionFinished()
	{
		GD.Print($"{Name} - interaction finished");
		if (Autocast)
			OnSpellReveal();
	}

	async protected virtual void OnSpellReveal()
	{
		GD.Print($"{Name} - spell reveal");
		if (SpellName != null && SpellName != "")
		{
			ui.RevealSpell(SpellName);
			await ToSignal(ui, "SpellRevealed");
			OnSpellRevealed();
		}
	}

	protected virtual void OnSpellRevealed()
	{
		GD.Print($"{Name} - spell revealed");
		GetTree().CallGroup("Player", "InteractableSpellRevealed", SpellName);
		OnSpellCast(SpellName);
	}

	protected virtual void OnSpellCast(string spellName)
	{
		GD.Print($"'{spellName}' is cast on '{Name}'");
		var equal = spellName.Equals(this.SpellName, StringComparison.OrdinalIgnoreCase);
		ui.RunInteraction(equal ? SpellInteractionLines : WrongSpellInteractionLines);
	}

	/* Functions to be called from outside via groups aka GetTree().CallGroup("Interactables", ...)
	since other interactables can be interested in performing actions depending on what is happening
	to an interactable in the current scene.
	
	Since 'CallGroup' calls funciton by the given name of EACH node in the given group
	I must check if the current Interactable is the given Interactable otherwise ALL
	interactables in the group will perform the same action. */

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
