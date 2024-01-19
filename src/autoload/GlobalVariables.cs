using Godot;
using System;

public partial class GlobalVariables : Node
{
    public bool GuardsDealtWith;
    public bool CloverTalkedTo;

    public override void _Ready()
    {
        SetToDefault();
    }

    public void SetToDefault()
    {
        GuardsDealtWith = false;
        CloverTalkedTo = false;
    }
}
