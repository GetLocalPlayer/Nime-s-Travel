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
            base.SpellName = revSpellName;
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
            base.SpellName = spellName;
        }
        else
            base.OnSpellCast(spellName);
    }

    protected override void OnSpellReveal()
    {
        base.OnSpellReveal();
        OnSpellCast(SpellName);
        ui.RunInteraction(isLevitating ? onCastLines : onRevCastLines);
        GetTree().CallGroup("Player", "InteractableSpellRevealed", spellName);
        GetTree().CallGroup("Player", "InteractableSpellRevealed", revSpellName);
        ui.BlockMouse();
    }

    protected override void OnSpellRevealed()
    {
        ui.UnblockMouse();
    }
}
