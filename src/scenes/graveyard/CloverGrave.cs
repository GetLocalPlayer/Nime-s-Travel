using Godot;
using System;
using System.Linq;

public partial class CloverGrave : Interactable
{
    [Export] string[] postDialogInteractinLines;

    AnimationPlayer headAnimPlayer;

    public override void _Ready()
    {
        base._Ready();
        headAnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    async protected override void OnInspection()
    {
        var globals = GetTree().Root.GetNode<GlobalVariables>("GlobalVariables");
        if (globals.GuardsDealtWith && !globals.CloverTalkedTo)
        {
            globals.CloverTalkedTo = true;
            InteractionLines = postDialogInteractinLines;
            ui.BlockMouse();
            headAnimPlayer.Play("CloverAppears");
            await ToSignal(headAnimPlayer, AnimationPlayer.SignalName.AnimationFinished);
            ui.UnblockMouse();

            ui.RunInteraction(FirstInteracitonLines.Take(FirstInteracitonLines.Length - 1).ToArray());
            await ToSignal(ui, "InteractionFinished");
            
            ui.BlockMouse();
            headAnimPlayer.PlayBackwards("CloverAppears");
            await ToSignal(headAnimPlayer, AnimationPlayer.SignalName.AnimationFinished);
            ui.UnblockMouse();

            ui.RunInteraction(FirstInteracitonLines.Skip(FirstInteracitonLines.Length - 1).ToArray());

            ui.ShowHornButtons("r");
        }
        else
            base.OnInspection();
    }

    public void RunDialog()
    {
        OnInspection();
    }
}
