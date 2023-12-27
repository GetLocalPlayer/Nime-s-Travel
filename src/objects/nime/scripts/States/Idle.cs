using Godot;
using System;

public partial class Idle : State
{
    public override void Enter(Node context)
    {
        var nime = (Node2D)context;
		var animPlayer = nime.GetNode<AnimatedSprite2D>("Animations");
        animPlayer.Play("Idle");
    }

    public override void Exit(Node context){}
    public override void Update(Node context, double delta){}
}
