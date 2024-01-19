using Godot;
using System;


/* A node to scale Nime depending on her Y position on the screen.

Root
|-- MaxScale    // contains max scale in its transfrom
|-- MinScale    // contains min scale in its transfrom

In each frame, the node interpolates the scale between transform
properties of MaxScale and MinScale markers depending on Y global
position of Nime betwee these two markers. */
public partial class Scaler : Node2D
{
    Marker2D max;
    Marker2D min;

    public override void _Ready()
    {
        max = GetNode<Marker2D>("MaxScale");
        min = GetNode<Marker2D>("MinScale");
        GetParent().ChildEnteredTree += (node) =>
        {
            if (node.IsInGroup("Player"))
                _Process(0);
        };
    }

    public override void _Process(double delta)
    {
        foreach (var node in GetTree().GetNodesInGroup("Player"))
        {
            var nime = node as Nime;
            var factor = (nime.GlobalPosition.Y - min.GlobalPosition.Y) / (max.GlobalPosition.Y - min.GlobalPosition.Y);
            nime.Scale = (min.Scale + (max.Scale - min.Scale) * factor) * nime.Scale.Sign();
        }
    }
}
