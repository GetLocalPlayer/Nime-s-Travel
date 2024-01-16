using Godot;
using System;
using System.Collections.Generic;

public partial class GraveyardSceneGate : SceneGate
{
    public override void _Ready()
    {
        base._Ready();
        TreeEntered += () =>
        {
            if (GetTree().Root.GetNode<GlobalVariables>("GlobalVariables").GuardsDealtWith)
            {
                IsBlocked = true;
            };
        };
    }
}
