using Godot;
using System;
using System.Collections.Generic;

public partial class CastMagic : State
{
    private Dictionary<char, string> animationNames = new Dictionary<char, string>{
        {'r', "CastRed"},
        {'g', "CastGreen"},
        {'b', "CastBlue"},
    };

    private int spellLength;

    public override void Enter(Node context)
    {
        var nime = (Nime)context;
		var animPlayer = nime.GetNode<AnimationPlayer>("AnimationPlayer");
        animPlayer.Stop(true);
        animPlayer.Play(animationNames[nime.Spell[0]]);
        spellLength = 1;
    }

    public override void Exit(Node context)
    {
        var nime = (Nime)context;
        var animPlayer = nime.GetNode<AnimationPlayer>("AnimationPlayer");
        animPlayer.Stop(true);
        animPlayer.ClearQueue();
        nime.GetNode<Sprite2D>("MagicSpark").Visible = false;
        nime.Spell = "";
    }

    public override void Update(Node context, double delta)
    {
        var nime = (Nime)context;
        if (nime.Spell.Length > spellLength)
        {
            var animPlayer = nime.GetNode<AnimationPlayer>("AnimationPlayer");
            for (; spellLength < nime.Spell.Length; spellLength++)
            {
                animPlayer.Queue(animationNames[nime.Spell[spellLength]]);
            }
        }
    }
}