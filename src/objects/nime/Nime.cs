using Godot;
using System;

public partial class Nime : Node2D
{
	[Export] public float MoveSpeed = 80;
	// Called when the node enters the scene tree for the first time.

	public void SetNewWalkTarget(Vector2 newTarget)
	{
		var agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		agent.TargetPosition = newTarget;
		// Только чтобы заставить логику поиска путей
		// работать и спровоцировать сигнал PathChanged 
		agent.GetNextPathPosition();
	}

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
