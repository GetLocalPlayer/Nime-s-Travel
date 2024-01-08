using Godot;
using System;

public partial class MagicEffect : Sprite2D
{
	private AnimationTree animTree;

	public override void _Ready()
	{
		animTree = GetNode<AnimationTree>("AnimationTree");
		animTree.Set("parameters/BlendResetPlay/blend_amount", 0);
		animTree.Set("parameters/ResetTime/seek_request", 0);
	}

	public void Appear()
	{
		animTree.Set("parameters/BlendResetPlay/blend_amount", 1);
		animTree.Set("parameters/BlendAppearDisppear/blend_amount", 0);
		animTree.Set("parameters/AppearTime/seek_request", 0);
	}

	public void Disappear()
	{
		animTree.Set("parameters/BlendResetPlay/blend_amount", 1);
		animTree.Set("parameters/BlendAppearDisppear/blend_amount", 1);
		animTree.Set("parameters/AppearTime/seek_request", 0);
	}
}
