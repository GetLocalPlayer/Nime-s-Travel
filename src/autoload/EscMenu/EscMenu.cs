using Godot;
using System.Collections.Generic;

public partial class EscMenu : Control
{
    [Export] int MouseHoverOutline = 2;
    [Export] int MouseHoverSizeIncrease = 3;

    [Export] string NewGameScenePath;
    
    public Dictionary<string, Button> Buttons = new();

    public override void _Ready()
    {
        var buttonContainer = GetNode("ButtonContainer");            

        foreach (var child in buttonContainer.GetChildren())
        {
            var btn = child.GetNode("Button") as Button;
            var minSize = (int)btn.Get("theme_override_font_sizes/font_size");
            var maxSize = minSize + MouseHoverSizeIncrease;
            var minOutline = (int)btn.Get("theme_override_constants/outline_size");
            var maxOutline = minOutline + MouseHoverOutline;
            btn.MouseEntered += () =>
            {
                btn.Set("theme_override_constants/outline_size", maxOutline);
                btn.Set("theme_override_font_sizes/font_size", maxSize);
            };
            btn.MouseExited += () =>
            {
                btn.Set("theme_override_constants/outline_size", minOutline);
                btn.Set("theme_override_font_sizes/font_size", minSize);
            };
            Buttons.Add(child.Name, btn);
        }

        buttonContainer.GetNode<Button>("NewGame/Button").Pressed += () =>
        {
            GetTree().ChangeSceneToFile(NewGameScenePath);
            GetTree().Root.GetNode<GlobalVariables>("GlobalVariables").SetToDefault();
        };

        buttonContainer.GetNode<Button>("Continue/Button").Pressed += () =>
            Hide();

        buttonContainer.GetNode<Button>("Language/Button").Toggled += (bool toggledOn) =>
            TranslationServer.SetLocale(toggledOn ? "en" : "ru");

        buttonContainer.GetNode<Button>("Music/Button").Toggled += (bool toggledOn) =>
            AudioServer.SetBusMute(AudioServer.GetBusIndex("MusicMaster"), !toggledOn);

        buttonContainer.GetNode<Button>("Sound/Button").Toggled += (bool toggledOn) =>
            AudioServer.SetBusMute(AudioServer.GetBusIndex("SoundEffect"), !toggledOn);

        buttonContainer.GetNode<Button>("Exit/Button").Pressed += () =>
            GetTree().Quit();

        var helpBackdrop = GetNode<Button>("HelpBackdrop");
        helpBackdrop.Hide();
        buttonContainer.GetNode<Button>("Help/Button").Pressed += () =>
            helpBackdrop.Show();
        helpBackdrop.Pressed += () =>
            helpBackdrop.Hide();

        var unpausedMouseMode = Input.MouseModeEnum.Visible;
        VisibilityChanged += () =>
        {
            GetTree().Paused = Visible;
            if (Visible)
            {
                unpausedMouseMode = Input.MouseMode;
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = unpausedMouseMode;
            }
        };

        Hide();
    }

    public void ShowPauseBackdrop()
    {
        GetNode<Control>("PauseBackdrop").Show();
    }

    public void HidePauseBackdrop()
    {
        GetNode<Control>("PauseBackdrop").Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Keycode == Key.Escape && key.Pressed && !key.Echo)
            if (Visible)
                Hide();
            else 
                Show();
    }
}
