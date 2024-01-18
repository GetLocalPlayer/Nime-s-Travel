using Godot;
using System;

public partial class GameOver : Node2D
{
    [Export] string text;

    public override void _Ready()
    {
        GetTree().Root.GetNode<UI>("UI").SetInteracitonText(text);
    }
}
