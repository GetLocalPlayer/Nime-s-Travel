using Godot;
using System;
using System.Linq;

public partial class CloverGrave : Interactable
{
    [Export] string[] cloverDialogLines;
    [Export] string[] postDialogInteractinLines;

    AnimationPlayer headAnimPlayer;

    public override void _Ready()
    {
        base._Ready();
        headAnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    async void RunDialog()
    {
           var backgroundMusic = GetTree().Root.GetNode("BackgroundMusic");
           backgroundMusic.GetNode<AudioStreamPlayer>("Default").Play();
           backgroundMusic.GetNode<AudioStreamPlayer>("Guards").Stop();
           
           ui.BlockMouse();
           headAnimPlayer.Play("CloverAppears");
           await ToSignal(headAnimPlayer, AnimationPlayer.SignalName.AnimationFinished);
           ui.UnblockMouse();

           ui.RunInteraction(cloverDialogLines.Take(cloverDialogLines.Length - 1).ToArray());
           await ToSignal(ui, "InteractionFinished");
           
           ui.BlockMouse();
           headAnimPlayer.PlayBackwards("CloverAppears");
           await ToSignal(headAnimPlayer, AnimationPlayer.SignalName.AnimationFinished);
           ui.UnblockMouse();
           ui.RunInteraction(cloverDialogLines.Skip(cloverDialogLines.Length - 1).ToArray());
           ui.ShowHornButtons("r");
    }

    async protected override void OnInspection()
    {
        var globals = GetTree().Root.GetNode<GlobalVariables>("GlobalVariables");
        if (globals.GuardsDealtWith && !globals.CloverTalkedTo)
        {
            InteractionLines = postDialogInteractinLines;
            globals.CloverTalkedTo = true;
            ui.RunInteraction(cloverDialogLines.Take(1).ToArray());
            await ToSignal(ui, "InteractionFinished");
            cloverDialogLines = cloverDialogLines.Skip(1).ToArray();
            RunDialog();
            
        }
        else
            base.OnInspection();
    }

    public void RunDialogFromBush()
    {
        cloverDialogLines = cloverDialogLines.Skip(1).ToArray();
        InteractionLines = postDialogInteractinLines;
        GetTree().Root.GetNode<GlobalVariables>("GlobalVariables").CloverTalkedTo = true;
        RunDialog();
    }
}
