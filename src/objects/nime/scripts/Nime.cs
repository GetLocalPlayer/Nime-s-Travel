using Godot;
using System;

public partial class Nime : Node2D
{
	[Signal] public delegate void FocusSetEventHandler();
	[Signal] public delegate void WalkTargetSetEventHandler();
	[Signal] public delegate void SceneEnteredEventHandler();

	[Export] public float MoveSpeed = 80;	
	public Interactable FocusedInteractable;

    public void SetNewWalkTarget(Vector2 newTarget)
	{
		GetNode<NavigationAgent2D>("NavigationAgent2D").TargetPosition = newTarget;
		EmitSignal("WalkTargetSet");
	}

	public void SetFocus(Interactable i)
	{
		if (i != null)
		{
			FocusedInteractable = i;
			GetNode<NavigationAgent2D>("NavigationAgent2D").TargetPosition = FocusedInteractable.WayPoint;
			EmitSignal("FocusSet");
		}
	}

	public void EnterScene(SceneGate gate)
	{
		Position = gate.SpawnPoint;
		GetNode<NavigationAgent2D>("NavigationAgent2D").TargetPosition = gate.SpawnWayPoint;
		EmitSignal("SceneEntered");
	}
}
