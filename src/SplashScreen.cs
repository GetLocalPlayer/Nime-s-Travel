using Godot;
using System;
using System.Collections.Generic;

public partial class SplashScreen : Control
{
    [Export] string nextScenePath;

    public override void _EnterTree()
    {
        var tree = GetTree();
        tree.Paused = true;
        var root = tree.Root;
        root.GetNode<Control>("UI").Hide();
        root.GetNode<Control>("EscMenu").Hide();

        /* Disabling all processes forcefully during splashscreen. */
        var processModes = new Dictionary<Node, ProcessModeEnum>();
        foreach (var child in root.GetChildren())
        {
            if (child != tree.CurrentScene)
            {
                processModes.Add(child, child.ProcessMode);
                child.ProcessMode = ProcessModeEnum.Disabled;
            }
        }

        Ready += () =>
        {
            GetNode<AnimationPlayer>("AnimationPlayer").AnimationFinished += (animName) =>
            {
                root.GetNode<Control>("UI").Hide();
                root.GetNode<Control>("EscMenu").Hide();
                tree.Paused = false;
                tree.ChangeSceneToFile(nextScenePath);
                foreach (var entry in processModes)
                    entry.Key.ProcessMode = entry.Value;
            };
        };
    }
}
