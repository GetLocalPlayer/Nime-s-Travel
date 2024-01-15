using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CastMagic : State
{
    readonly static Dictionary<char, string> code2AnimName = new(){
        {'r', "CastRed"},
        {'g', "CastGreen"},
        {'b', "CastBlue"},
    };

    event AnimationMixer.AnimationFinishedEventHandler onAnimationFinished;
    readonly List<char> codes = new();
    double exitTime;

    public override void Enter(Node context)
    {
        codes.Clear();
        var nime = (Nime)context;
        exitTime = nime.SpellResetTime;
        nime.GetNode<Sprite2D>("MagicSpark").Show();
		var animPlayer = nime.GetNode<AnimationPlayer>("AnimationPlayer");
        var spells = nime.GetTree().Root.GetNode<Spells>("Spells");
    
        onAnimationFinished = (animName) =>
        {
            var spellName = spells.GetSpellName(new string(codes.ToArray()));
            if (nime.LearntSpells.Contains(spellName, StringComparer.OrdinalIgnoreCase))
            {
                Success(context, spellName);
                EmitStateFinished(context);
            }
            else
                exitTime = nime.SpellResetTime;
        };
        animPlayer.AnimationFinished += onAnimationFinished;
        nime.GetTree().CallGroup("Interactables", "StartCastOnInteractable", nime.TargetedInteractable);
    }

    protected virtual void Success(Node context, string spellName)
    {
        var nime = context as Nime;
        nime.GetTree().CallGroup("Interactables", "CastOnInteractable", nime.TargetedInteractable, spellName);
    }

    public override void Update(Node context, double delta)
    {
		var animPlayer = context.GetNode<AnimationPlayer>("AnimationPlayer");
        if (!animPlayer.IsPlaying())
        {
            exitTime -= delta;
            if (exitTime <= 0)
                EmitStateFinished(context);
        }
    }

    public void AddCode(Node context, char code)
    {
        var nime = context as Nime;
        exitTime = nime.SpellResetTime;
        var animPlayer = nime.GetNode<AnimationPlayer>("AnimationPlayer");
        if (!codes.Any())
            animPlayer.Play(code2AnimName[code]);
        else
            animPlayer.Queue(code2AnimName[code]);
        codes.Add(code);
    }

    public override void Exit(Node context)
    {
        var nime = (Nime)context;
        var animPlayer = nime.GetNode<AnimationPlayer>("AnimationPlayer");
        animPlayer.Stop(true);
        animPlayer.ClearQueue();
        nime.GetNode<Sprite2D>("MagicSpark").Hide();
        animPlayer.AnimationFinished -= onAnimationFinished;
        onAnimationFinished = null;
        codes.Clear();
        nime.GetTree().CallGroup("Interactables", "StopCastOnInteractable", nime.TargetedInteractable);
    }
}
