using Godot;
using System;
using System.Linq;

public partial class Guards : Interactable
{
    [Export] string[] revSpellLines;
    [Export] string[] afterSpellLines;
    [Export] NavigationRegion2D blockedRegion;

    public bool IsLevitating = false;
    SceneGate gatesToBlock;

    public override void _Ready()
    {
        base._Ready();
        blockedRegion.Enabled = !Visible;

        VisibilityChanged += () =>
        {
            if (Visible)
            {
                blockedRegion.Enabled = false;
                gatesToBlock.IsBlocked = true;
            }
        };
        CallDeferred("ReadyDeferred");
    }

    void ReadyDeferred()
    {
        var currentScene = GetTree().CurrentScene;
        gatesToBlock = currentScene.GetNode<SceneGate>("%ToStreetToHome");
        currentScene.ChildEnteredTree += (node) =>
        {
            if (node.Name != "Nime") return;
            var learntSpells = (node as Nime).LearntSpells;
            var comparer = StringComparer.OrdinalIgnoreCase;
            if (learntSpells.Contains("LEVITATION", comparer) && learntSpells.Contains("IGNITION", comparer))
                Show();
        };
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
                Met = true;
                base.SpellName = "-" + spellName;
                spellName = base.SpellName;
                var tree = GetTree();
                tree.Root.GetNode<GlobalVariables>("GlobalVariables").GuardsDealtWith = true;
                gatesToBlock.IsBlocked = false;
            }
        }
        base.OnSpellCast(spellName);
    }
}
