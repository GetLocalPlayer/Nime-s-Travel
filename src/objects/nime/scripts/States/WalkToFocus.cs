using Godot;
using System;
using System.Runtime.CompilerServices;


public class WalkToFocus : Walk
{
    protected  override void OnFinish(Node context)
    {
        var nime = (Nime)context;
        var lookAtPoint = nime.Focus.LookAtPoint;
        nime.Scale = nime.Scale with { X = Math.Abs(nime.Scale.X) * (nime.Position.X > lookAtPoint.X ? -1f : 1) };
        base.OnFinish(nime);
    }
}