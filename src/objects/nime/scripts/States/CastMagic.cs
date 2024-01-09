using Godot;
using System;
using System.Collections.Generic;

public partial class CastMagic : State
{
    public delegate void SpellCastHandler(State state, Node context, string spell);
    public event SpellCastHandler SpellCast;

    readonly static Dictionary<char, string> code2AnimName = new Dictionary<char, string>{
        {'r', "CastRed"},
        {'g', "CastGreen"},
        {'b', "CastBlue"},
    };

    event AnimationMixer.AnimationFinishedEventHandler onAnimationFinished;
    string spellBeingCast;
    double exitTime;

    public override void Enter(Node context)
    {
        spellBeingCast = "";
        var nime = (Nime)context;
        nime.GetNode<Sprite2D>("MagicSpark").Show();
		var animPlayer = nime.GetNode<AnimationPlayer>("AnimationPlayer");
        var spellLength = 1;
        onAnimationFinished = (animName) =>
        {
            if (spellLength < spellBeingCast.Length)
            {
                animPlayer.Play(code2AnimName[spellBeingCast[spellLength]]);
                spellLength++;
            }
            else if (nime.SpellBook.ContainsKey(spellBeingCast))
            {
                SpellCast?.Invoke(this, context, spellBeingCast);
                EmitStateFinished(context);
            }
            else
                exitTime = nime.SpellResetTime;
        };
        animPlayer.AnimationFinished += onAnimationFinished;
    }

    public override void Update(Node context, double delta)
    {
        var nime = (Nime)context;
		var animPlayer = nime.GetNode<AnimationPlayer>("AnimationPlayer");
        if (!animPlayer.IsPlaying())
        {
            exitTime -= delta;
            if (exitTime <= 0)
            {
                spellBeingCast = "";
                EmitStateFinished(context);
            }
        }
    }

    public void AddSpell(Node context, string code)
    {
        var animPlayer = (context as Nime).GetNode<AnimationPlayer>("AnimationPlayer");
        if (spellBeingCast == "" || !animPlayer.IsPlaying())
            (context as Nime).GetNode<AnimationPlayer>("AnimationPlayer").Play(code2AnimName[code[0]]);
        spellBeingCast += code;
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
        spellBeingCast = "";
    }
}
