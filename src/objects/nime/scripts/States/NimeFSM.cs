using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class NimeFSM : FiniteStateMachine
{	
	 Nime nime;

	private readonly Dictionary<string, State> states = new Dictionary<string, State> {
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
			GetTree().CallGroup("Interactables", "InteractableReached", nime.TargetedInteractable);
			GetTree().CallGroup("UI", "InteractableReached", nime.TargetedInteractable);
		};

		nime.MagicCast += () =>
			SetState(states["castMagic"]);

		states["castMagic"].StateFinished += (state, context) =>
			SetState(states["idle"]);
	}
}
