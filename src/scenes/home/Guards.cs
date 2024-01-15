using Godot;
using System;

public partial class Guards : Interactable
{
    [Export] string[] revSpellLines;
    [Export] string[] afterSpellLines;
    [Export] NavigationRegion2D blockedRegion;

    public bool IsLevitating = false;

    public override void _Ready()
    {
        base._Ready();
        blockedRegion.Enabled = !Visible;
        VisibilityChanged += () => blockedRegion.Enabled = !Visible;
    }

    async protected override void OnSpellCast(string spellName)
    {
        if (spellName.Equals(base.SpellName, StringComparison.OrdinalIgnoreCase))
        {
            /* Обратные заклинания имеют префикс в виде
            минуса. */
            if (spellName.StartsWith("-"))
            {
                ui.RunInteraction(revSpellLines);
                return;
            }
            else
            {
                ui.BlockMouse();
                var player = GetNode<AnimationPlayer>("AnimationPlayer");
                player.Play("FlyUp");
                await ToSignal(player, AnimationPlayer.SignalName.AnimationFinished);
                ui.UnblockMouse();
                player.Play("Levitation");
                InteractionLines = afterSpellLines;
                IsLevitating = true;
                met = true;
                base.SpellName = "-" + spellName;
                spellName = base.SpellName;
                GetTree().Root.GetNode<GlobalVariables>("GlobalVariables").GuardsDealtWith = true;
            }
        }
        base.OnSpellCast(spellName);
    }
}
