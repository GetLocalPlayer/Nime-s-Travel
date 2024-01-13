using Godot;
using System;

public partial class LevitationFlask : Interactable
{
    [ExportGroup("Levitation Spell")]
    [Export] string spellName;
    [Export] string revSpellName;
    [Export] string[] onCastLines;
    [Export] string[] alreadyUsedLines;
    [Export] string[] onRevCastLines;
    [Export] string[] revAlreadyUsedLines;
    bool isLevitating = false;
    AnimationPlayer animPlayer;

    public override void _Ready()
    {
        base._Ready();
        base.SpellName = spellName;
        animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }


    protected override void OnSpellCast(string spellName)
    {
        if (spellName == this.spellName)
        {
            if (isLevitating)
            {
                ui.RunInteraction(alreadyUsedLines);
                return;
            }
            isLevitating = true;
            animPlayer.Play("FlyUp");
            animPlayer.Queue("Levitation");
        }
        else if (spellName == this.revSpellName)
        {
            if (!isLevitating)
            {
                ui.RunInteraction(revAlreadyUsedLines);
                return;
            }
            isLevitating = false;
            animPlayer.Play("Land");
            animPlayer.Queue("Idle");
        }
        else
            base.OnSpellCast(spellName);
    }

    async protected override void OnSpellRevealed()
    {
        OnSpellCast(isLevitating ? revSpellName : spellName);
        ui.BlockMouse();
        await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationChanged);
        ui.UnblockMouse();
        ui.RunInteraction(isLevitating ? onCastLines : onRevCastLines);
        GetTree().CallGroup("Player", "InteractableSpellRevealed", spellName);
        GetTree().CallGroup("Player", "InteractableSpellRevealed", revSpellName);
        /* Базовый класс должен иметь значение в поле SpellName
        для демонстрации заклинания. */
        base.SpellName = isLevitating ? revSpellName : spellName;
    }
}
