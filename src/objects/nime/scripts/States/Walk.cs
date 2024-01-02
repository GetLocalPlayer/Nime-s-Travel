using Godot;
using System;

public partial class Walk : State
{
    public override void Enter(Node context)
    {
        var nime = (Nime)context;
        nime.GetNode<AnimationPlayer>("AnimationPlayer").Play("Walk");
        nime.GetNode<AudioStreamPlayer2D>("GalopSound").Play();
        var agent = nime.GetNode<NavigationAgent2D>("NavigationAgent2D");
        var pos = nime.GlobalPosition;
        var nextPos = agent.GetNextPathPosition();
        nime.Scale = nime.Scale with { X = Math.Abs(nime.Scale.X) * (pos.X > nextPos.X ? -1f : 1) };
        if (agent.IsNavigationFinished())
            Finalize(nime);
    }

    public override void Exit(Node context)
    {
        var nime = (Nime)context;
        nime.GetNode<AudioStreamPlayer2D>("GalopSound").Stop();
    }

    public override void Update(Node context, double delta)
    {
        
        var nime = (Nime)context;
        var agent = nime.GetNode<NavigationAgent2D>("NavigationAgent2D");
        var nextPos = agent.GetNextPathPosition();
        var offset = (nextPos - nime.GlobalPosition).Normalized() * (float)delta * nime.MoveSpeed;
        nime.GlobalPosition += offset;
        nime.Scale = nime.Scale with { X = Math.Abs(nime.Scale.X) * (offset.X < 0f ? -1f : 1) };
        if (agent.IsNavigationFinished())
            Finalize(nime);
    }

    protected virtual void Finalize(Node context)
    {
        EmitStateFinished(context);
    }
}
