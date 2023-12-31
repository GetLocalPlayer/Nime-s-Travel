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
		{"walkToFocus", new WalkToFocus()},
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
			SetState(states["walk"]);

		states["walk"].StateFinished += (state, context) =>
			SetState(states["idle"]);

		nime.SceneEntered += () =>	
			SetState(states["walk"]);

		nime.FocusSet += () =>
		{
			SetState(states["walkToFocus"]);
		};

		states["walkToFocus"].StateFinished += (state, context) =>
			SetState(states["idle"]);
	}
}
