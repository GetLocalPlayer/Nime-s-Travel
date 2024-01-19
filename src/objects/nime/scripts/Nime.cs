using Godot;
using System;
using System.Collections.Generic;

public partial class Nime : Node2D
{
	/* All signals are defined specifically to use them inside
	Nime's finite state machine (NimeFSM). Not sure, maybe
	regular funciton calling would be better. */
	[Signal] public delegate void InteractableTargetedEventHandler();
	[Signal] public delegate void WalkTargetSetEventHandler();
	[Signal] public delegate void SceneEnteredEventHandler();
	[Signal] public delegate void HornUsedEventHandler(char code);
	[Signal] public delegate void OnLookAtEventHandler(Vector2 target);

	[Export] public float MoveSpeed = 80;

	/* Time before spell cast sequence is reset. */
	[Export] public float SpellResetTime = 2;

	public Interactable TargetedInteractable;

	/* An array of spells known by Nime on game start. You can
	check all possible spell name in "autoload/Spells.cs".
	Used mainly for testing. */
	[Export] string[] initialSpells;

	/* The spells Nime learnt. */
	public List<string> LearntSpells = new();

    public override void _Ready()
    {
		if (initialSpells != null)
        	LearntSpells.AddRange(initialSpells);
    }

	/* Enable/disable collider and process mode. */
	public void Enable()
	{
		ProcessMode = ProcessModeEnum.Inherit;
		var col = GetNode<Area2D>("Collider");
		col.Monitorable = true;
		col.Monitoring = true;
	}

	public void Disable()
	{
		ProcessMode = ProcessModeEnum.Disabled;
		var col = GetNode<Area2D>("Collider");
		col.Monitorable = false;
		col.Monitoring = false;
	}

	public new void LookAt(Vector2 target)
	{
		EmitSignal("OnLookAt", target);
	}


    /* Functions to be called via group call GetTree().CallGroup("Player").
	Technically, a group call means EACH node in the group will invoke
	a method by the given name and I must check inside this method if
	it belongs to the correct instance. But I don't do this check since
	I have only one Nime active at a time in the current scene. 	*/

    /* Called from Background. */

    public void BackgroundClicked(Vector2 newTarget)
	{
		var agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		agent.TargetPosition = newTarget;
		EmitSignal("WalkTargetSet");
		if (TargetedInteractable != null)
		{ 
			GetTree().CallGroup("Interactables", "InteractableLeft", TargetedInteractable);			
			TargetedInteractable = null;
		}
	}

	/* Called from Interactable. */

	public void InteractableClicked(Interactable i)
	{
		if (TargetedInteractable != i)
		{
			if (TargetedInteractable != null)
			{
				GetTree().CallGroup("Interactables", "InteractableLeft", TargetedInteractable);
			}
			TargetedInteractable = i;
			GetNode<NavigationAgent2D>("NavigationAgent2D").TargetPosition = TargetedInteractable.WayPoint;
			EmitSignal("InteractableTargeted");
		}
	}
	public void InteractableSpellRevealed(string spellName)
	{
		if (!LearntSpells.Contains(spellName))
			LearntSpells.Add(spellName);
	}

	/* CallGroup("Player", ...) из класса SceneGate. */
	public void EnterScene(SceneGate gate)
	{
		Position = gate.SpawnPoint;
		GetNode<NavigationAgent2D>("NavigationAgent2D").TargetPosition = gate.SpawnWayPoint;
		EmitSignal("SceneEntered");
	}

	public void SceneGateClicked(SceneGate g)
	{
		BackgroundClicked(g.ExitWayPoint);
	}


	/* Called from UI. */

	public void HornButtonClicked(char code)
	{
		EmitSignal("HornUsed", code);
	}
}
