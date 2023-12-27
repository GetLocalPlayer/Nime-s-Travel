using Godot;
using System;

public partial class Nime : Node2D
{
	[Export] public float MoveSpeed = 80;

	public void SetNewWalkTarget(Vector2 newTarget)
	{
		var agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		agent.TargetPosition = newTarget;
		// Вызов GetNextPathPosition только чтобы
		// спровоцировать сигнал PathChanged в ноде
		// NavigationAgend2D и последующие действия
		// NimeFSM.
		agent.GetNextPathPosition();
	}
}
