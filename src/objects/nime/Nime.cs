using Godot;
using System;

public partial class Nime : Node2D
{
	[Export] public float MoveSpeed = 80;

    public void SetNewWalkTarget(Vector2 newTarget)
	{
		var agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		agent.TargetPosition = newTarget;
		agent.EmitSignal(NavigationAgent2D.SignalName.PathChanged);
	}


	public void EnterScene(Vector2 spawnPoint, Vector2 wayPoint)
	{
		GlobalPosition = spawnPoint;
		var agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		agent.TargetPosition = wayPoint;
		agent.EmitSignal(NavigationAgent2D.SignalName.PathChanged);
		Scale = Scale with { X = Math.Abs(Scale.X) * (spawnPoint.X > wayPoint.X ? -1f : 1) };
	}
}
