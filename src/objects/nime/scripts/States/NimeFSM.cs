using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class NimeFSM : FiniteStateMachine
{	
	private readonly Dictionary<string, State> States = new Dictionary<string, State> {
		{"idle", new Idle()},
		{"walk", new Walk()},
	};

    public override void _Ready()
	{
		States["walk"].StateFinished += (state, context) => 
			SetState(States["idle"]);
		
		var agent = GetParent().GetNode<NavigationAgent2D>("NavigationAgent2D");
		agent.PathChanged += () =>
		{
			
			SetState(States["walk"]);
		};

		SetState(States["idle"]);
	}
}
