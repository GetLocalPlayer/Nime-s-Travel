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
        cloverGrave.RunDialogFromBush();
    }

    async protected override void OnSpellCast(string spellName)
    {
        if (spellName.Equals(this.SpellName, StringComparison.OrdinalIgnoreCase))
        {
            GetNode<Area2D>("Clickable").InputPickable = false;

            var backgroundMusic = GetTree().Root.GetNode("BackgroundMusic");
            var defaultPlayer = backgroundMusic.GetNode<AudioStreamPlayer>("Default");
            defaultPlayer.Stop();
            var firePlayer = backgroundMusic.GetNode<AudioStreamPlayer>("Fire");
            firePlayer.Play();

            var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            /* Там 3 последовательных анимаций, но переходы между
            ними заданы в редакторе AnimationPlayer посему тут не
            приходится впердоливать очередь анимаций кодом. */
            animPlayer.Play("FireGrow");
            ui.BlockMouse();    
            await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
            ui.UnblockMouse();
            firePlayer.Stop();
            defaultPlayer.Play();
            GetTree().CurrentScene.GetNode<NavigationRegion2D>("NavigationRegions/Escape").Enabled = true;
        }
        base.OnSpellCast(spellName);
    }
}
