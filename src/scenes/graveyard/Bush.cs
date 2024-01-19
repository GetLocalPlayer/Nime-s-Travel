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
            var tree = GetTree();
            var backgroundMusic = tree.Root.GetNode("BackgroundMusic");
            var defaultPlayer = backgroundMusic.GetNode<AudioStreamPlayer>("Default");
            defaultPlayer.Stop();
            var firePlayer = backgroundMusic.GetNode<AudioStreamPlayer>("Fire");
            firePlayer.Play();

            var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            /* FireGrow, GrayOut, and Decay are 3 different
            animations. Usually I call .Queue method but here
            it was possible to set transitions from FireGrow
            to the rest in the sequence via AnimationPlayer
            itself. */
            animPlayer.Play("FireGrow");
            ui.BlockMouse();    
            await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
            ui.UnblockMouse();
            firePlayer.Stop();
            defaultPlayer.Play();
            tree.CurrentScene.GetNode<NavigationRegion2D>("NavigationRegions/Escape").Enabled = true;
            tree.CurrentScene.GetNode<SceneGate>("%ToGameOver").Active = true;
        }
        base.OnSpellCast(spellName);
    }
}
