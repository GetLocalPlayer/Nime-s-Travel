using Godot;
using System;

public partial class EscMenu : CenterContainer
{
    [Export] int MouseHoverOutline = 2;
    [Export] int MouseHoverSizeIncrease = 3;


    public override void _Ready()
    {
        GetTree().Root.GetNode<Control>("UI").Hide();

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
        }

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

        
    }
}
