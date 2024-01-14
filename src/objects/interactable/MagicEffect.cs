using Godot;
using System;

public partial class MagicEffect : Sprite2D
{
	AnimationTree animTree;

	public override void _Ready()
	{
		animTree = GetNode<AnimationTree>("AnimationTree");
		animTree.Set("parameters/BlendResetPlay/blend_amount", 0);
		animTree.Set("parameters/ResetTimeSeek/seek_request", 0);
	}

	public void Appear()
	{
		animTree.Set("parameters/BlendResetPlay/blend_amount", 1);
		animTree.Set("parameters/AppearTimeScale/scale", 1);
		animTree.Set("parameters/AppearTimeSeek/seek_request", animTree.Get("parameters/Appear/time"));
	}

	public void Disappear()
	{
		animTree.Set("parameters/BlendResetPlay/blend_amount", 1);
		animTree.Set("parameters/AppearTimeScale/scale", -1);
		animTree.Set("parameters/AppearTimeSeek/seek_request", animTree.Get("parameters/Appear/time"));
	}
}
