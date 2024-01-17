using Godot;
using System;
using System.Collections.Generic;

public partial class StreetSceneGate : SceneGate
{
    public override void _EnterTree()
    {
        if (GetTree().Root.GetNode<GlobalVariables>("GlobalVariables").GuardsDealtWith)
        {
            IsBlocked = true;
        };
    }
}
