using Godot;
using System;

public partial class CustomCheckButton : Button
{
    [Export] string TextChacked;
    [Export] string TextUnchecked;

    public override void _Toggled(bool toggledOn)
    {
        Text = toggledOn ? TextChacked : TextUnchecked;
    }

    public override void _Ready()
    {
        _Toggled(ButtonPressed);
    }
}
