using Godot;
using System;

public partial class Nime : Node2D
{
	[Signal] public delegate void InteractableTargetedEventHandler();
	[Signal] public delegate void WalkTargetSetEventHandler();
	[Signal] public delegate void SceneEnteredEventHandler();
	[Signal] public delegate void MagicCastEventHandler();

	[Export] public float MoveSpeed = 80;	
	public Interactable TargetedInteractable;
	public string Spell = "";

    public void BackgroundClicked(Vector2 newTarget)
	{
		var agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		agent.TargetPosition = newTarget;
		EmitSignal("WalkTargetSet");
		if (TargetedInteractable != null)
		{ 
			GetTree().CallGroup("Interactables", "InteractableLeft", TargetedInteractable);
			GetTree().CallGroup("UI", "InteractableLeft", TargetedInteractable);
			TargetedInteractable = null;
		}
	}

	public void InteractableClicked(Interactable i)
	{
		if (TargetedInteractable != i)
		{
			if (TargetedInteractable != null)
			{
				GetTree().CallGroup("Interactables", "InteractableLeft", TargetedInteractable);
				GetTree().CallGroup("UI", "InteractableLeft", TargetedInteractable);
			}
			TargetedInteractable = i;
			GetNode<NavigationAgent2D>("NavigationAgent2D").TargetPosition = TargetedInteractable.WayPoint;
			EmitSignal("InteractableTargeted");
		}
	}

	public void EnterScene(SceneGate gate)
	{
		Position = gate.SpawnPoint;
		GetNode<NavigationAgent2D>("NavigationAgent2D").TargetPosition = gate.SpawnWayPoint;
		EmitSignal("SceneEntered");
	}

	public void HornButtonClicked(string buttonName)
	{
		Spell += buttonName[0].ToString().ToLower();
		EmitSignal("MagicCast");
		GetTree().CallGroup("Interactables", "SpellOnInteractable", TargetedInteractable);
	}
}
