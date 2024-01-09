using Godot;
using System;
using System.Collections.Generic;

public partial class Nime : Node2D
{
	/* Все сигналы сделаны исключитльено для использования
	внутри конечного автомата NimeFSM. Не уверем, может
	обычный выхов функции был бы лучше. */
	[Signal] public delegate void InteractableTargetedEventHandler();
	[Signal] public delegate void WalkTargetSetEventHandler();
	[Signal] public delegate void SceneEnteredEventHandler();
	[Signal] public delegate void MagicCastEventHandler(string magicName);

	[Export] public float MoveSpeed = 80;	

	/* Время перед сбросом заклинания. */
	[Export] public float SpellResetTime = 2;

	public Interactable TargetedInteractable;

	/* Код спелла а-ля rgg ggb и его название а-ля
	LEVITATION, IGNITION. Заполняется по мере открытия
	в мире. Все строки приводятся к нижнему регистру. */
	public readonly Dictionary<string, string> SpellBook = new Dictionary<string, string>{
		{"bbg", "levitation"}
	};

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
			GetTree().CallGroup("UI", "InteractableLeft", TargetedInteractable);
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
				GetTree().CallGroup("UI", "InteractableLeft", TargetedInteractable);
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
	public void HornButtonClicked(string buttonName)
	{
		EmitSignal("MagicCast", buttonName[0].ToString().ToLower());
	}
}
