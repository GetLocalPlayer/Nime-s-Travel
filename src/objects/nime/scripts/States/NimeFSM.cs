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
		};

		nime.HornUsed += (code) =>
		{
			if (currentState != states["castMagic"])
				SetState(states["castMagic"]);
			(currentState as CastMagic).AddCode(nime, code);
		};

		states["castMagic"].StateFinished += (state, context) =>
		{
			SetState(states["idle"]);
		};	
	}
}
