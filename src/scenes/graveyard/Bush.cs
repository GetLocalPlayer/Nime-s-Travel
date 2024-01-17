using Godot;
using Microsoft.VisualBasic;
using System;

public partial class Bush : Interactable
{
    protected override void OnInteraction()
    {
        var globals = GetTree().Root.GetNode<GlobalVariables>("GlobalVariables");
        if (globals.GuardsDealtWith && !globals.CloverTalkedTo)
        {
            OnFirstInteraction();
        }
        else
            base.OnInteraction();
    }

    protected override void OnFirstInteractionFinished()
    {
        var tree = GetTree();
        var cloverGrave = tree.CurrentScene.GetNode<CloverGrave>("Interactables/CloverGrave");
        tree.CallGroup("Player", "LookAt", cloverGrave.LookAtPoint);
        cloverGrave.RunDialog();
    }

    async protected override void OnSpellCast(string spellName)
    {
        if (spellName.Equals(this.SpellName, StringComparison.OrdinalIgnoreCase))
        {
            GetNode<Area2D>("Clickable").InputPickable = false;
            var player = GetNode<AnimationPlayer>("AnimationPlayer");
            /* Там 3 последовательных анимаций, но переходы между
            ними заданы в редакторе анимаий посему тут не приходится. */
            player.Play("FireGrow");
            ui.BlockMouse();
            await ToSignal(player, AnimationPlayer.SignalName.AnimationFinished);
            ui.UnblockMouse();
            GetTree().CurrentScene.GetNode<NavigationRegion2D>("NavigationRegions/Escape").Enabled = true;
        }
        base.OnSpellCast(spellName);
    }
}
