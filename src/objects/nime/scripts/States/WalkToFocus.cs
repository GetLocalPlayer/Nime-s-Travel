using Godot;
using System;
using System.Runtime.CompilerServices;


public class WalkToInteractable : Walk
{
    protected  override void Finalize(Node context)
    {
        var nime = (Nime)context;
        var lookAtPoint = nime.TargetedInteractable.LookAtPoint;
        nime.Scale = nime.Scale with { X = Math.Abs(nime.Scale.X) * (nime.Position.X > lookAtPoint.X ? -1f : 1) };
        base.Finalize(nime);
    }
}