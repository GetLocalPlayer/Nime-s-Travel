using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Nime : Node2D
{
	/* Все сигналы сделаны исключитльено для использования
	внутри конечного автомата NimeFSM. Не уверем, может
	обычный выхов функции был бы лучше. */
	[Signal] public delegate void InteractableTargetedEventHandler();
	[Signal] public delegate void WalkTargetSetEventHandler();
	[Signal] public delegate void SceneEnteredEventHandler();
	[Signal] public delegate void HornUsedEventHandler(char code);

	[Export] public float MoveSpeed = 80;	

	/* Время перед сбросом заклинания. */
	[Export] public float SpellResetTime = 2;

	public Interactable TargetedInteractable;

	/* Названия изученный заклинаний ака
	LEVITATION, IGNITION. Существующие
	заклинания можно посмотреть в
	Autoload/Spells. */
	[Export] public string[] LearntSpells;

	/*
	Функции ниже вызываются через GetTree().CallGroup("Player", ...).
	Поскольку в группе Player может быть только один нод Nime, я
	не заморачиваюсь проверкой если текущий нод находится под
	управлением игрока.
	*/

	/* CallGroup("Player", ...) вызывается из класса
	Background. */
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

	/* CallGroup("Player", ...) из класса Interactable 
	и класса UI (клик на иконку в углу). */
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

	/* CallGroup("Player", ...) из класса SceneGate. */
	public void EnterScene(SceneGate gate)
	{
		Position = gate.SpawnPoint;
		GetNode<NavigationAgent2D>("NavigationAgent2D").TargetPosition = gate.SpawnWayPoint;
		EmitSignal("SceneEntered");
	}

	/* CallGroup("Player", ...) из класса UI. */
	public void HornButtonClicked(char code)
	{
		EmitSignal("HornUsed", code);
	}

	public void InteractableSpellRevealed(string spellName)
	{
		if (!LearntSpells.Contains(spellName))
			LearntSpells.Append(spellName);
	}
}
