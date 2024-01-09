using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class NimeFSM : FiniteStateMachine
{	
	Nime nime;

	readonly Dictionary<string, State> states = new Dictionary<string, State> {
		{"idle", new Idle()},
		{"walk", new Walk()},
		{"walkToInteractable", new WalkToInteractable()},
		{"castMagic", new CastMagic()},
	};

    public override void _Ready()
	{
		nime = (Nime)GetContext();
		SetState(states["idle"]);
		SetupTransitions();
	}

	private void SetupTransitions()
	{
		var tree = GetTree();

		nime.WalkTargetSet += () =>
		{
			SetState(states["walk"]);
		};

		states["walk"].StateFinished += (state, context) =>
			SetState(states["idle"]);

		nime.SceneEntered += () =>	
			SetState(states["walk"]);

		nime.InteractableTargeted += () =>
		{
			SetState(states["walkToInteractable"]);
		};

		states["walkToInteractable"].StateFinished += (state, context) =>
		{
			SetState(states["idle"]);
			tree.CallGroup("Interactables", "InteractableReached", nime.TargetedInteractable);
			tree.CallGroup("UI", "InteractableReached", nime.TargetedInteractable);
		};

		nime.MagicCast += (spellCode) =>
		{
			if (currentState != states["castMagic"])
				SetState(states["castMagic"]);
			(currentState as CastMagic).AddSpell(nime, spellCode);
			tree.CallGroup("Interactables", "StartCastOnInteractable", nime.TargetedInteractable);
		};

		(states["castMagic"] as CastMagic).SpellCast += (state, context, spellBeingCast) =>
		{
			tree.CallGroup("Interactables", "CastOnInteractable", nime.TargetedInteractable, spellBeingCast);
		};

		states["castMagic"].StateFinished += (state, context) =>
		{
			SetState(states["idle"]);
			tree.CallGroup("Interactables", "StopCastOnInteractable", nime.TargetedInteractable);
		};
	}
}
