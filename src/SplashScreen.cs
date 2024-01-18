using Godot;
using System;

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
        Ready += () =>
        {
            GetNode<AnimationPlayer>("AnimationPlayer").AnimationFinished += (animName) =>
            {
                root.GetNode<Control>("UI").Hide();
                root.GetNode<Control>("EscMenu").Hide();
                tree.Paused = false;
                tree.ChangeSceneToFile(nextScenePath);
            };
        };
    }
}
