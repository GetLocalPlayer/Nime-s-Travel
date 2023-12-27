using Godot;
using System;

public partial class Walk : State
{
    public override void Enter(Node context)
    {
        var nime = (Nime)context;
        nime.GetNode<AnimatedSprite2D>("Animations").Play("Walk");
        var audioPlayer = nime.GetNode<AudioStreamPlayer2D>("GalopSound");
        if (!audioPlayer.Playing) audioPlayer.Play();
        var animSprite = nime.GetNode<AnimatedSprite2D>("Animations");
        animSprite.Play("Walk");
    }

    public override void Exit(Node context)
    {
        var nime = (Nime)context;
        var audioPlayer = nime.GetNode<AudioStreamPlayer2D>("GalopSound");
        audioPlayer.Stop();
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
            RaiseStateFinished(nime);
    }
}
