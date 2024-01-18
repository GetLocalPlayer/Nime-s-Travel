using Godot;
using System;
using System.Runtime.InteropServices;

public partial class main : TextureRect
{
    [Export] string IntroScenePath;

    public override void _Ready()
    {
        var tree = GetTree();

        var ui = tree.Root.GetNode<Control>("UI");
        ui.Hide();

        var escMenu = tree.Root.GetNode<EscMenu>("EscMenu");
        escMenu.Show();
        escMenu.HidePauseBackdrop();
 
        var buttonContainer = escMenu.GetNode<Control>("ButtonContainer");
        buttonContainer.Position = buttonContainer.Position with {Y = buttonContainer.Position.Y + 50};

        var buttonsBackdrop = escMenu.GetNode<Control>("ButtonsBackdrop");
        buttonsBackdrop.Position = buttonsBackdrop.Position with {Y = buttonsBackdrop.Position.Y + 50};

        escMenu.Buttons["Continue"].GetParent<Control>().Hide();
        
        escMenu.Buttons["NewGame"].Pressed += () =>
        {
            escMenu.Buttons["NewGame"].GetParent<Control>().Hide();
            escMenu.ShowPauseBackdrop();
            escMenu.Hide();
            tree.ChangeSceneToFile(IntroScenePath);
            buttonContainer.Position = buttonContainer.Position with {Y = buttonContainer.Position.Y - 80};
            buttonsBackdrop.Position = buttonsBackdrop.Position with {Y = buttonsBackdrop.Position.Y - 80};
            escMenu.Buttons["Continue"].Pressed += () =>
                escMenu.Hide();
            escMenu.EscapePressed += () =>
                escMenu.Visible = !escMenu.Visible;
            escMenu.VisibilityChanged += () =>
            {
                var processMode = escMenu.Visible ? ProcessModeEnum.Disabled : ProcessModeEnum.Always;
                tree.CurrentScene.ProcessMode = processMode;
                ui.ProcessMode = processMode;
            };
        };        
    }
}
