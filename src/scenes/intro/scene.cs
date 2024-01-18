using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class scene : Button
{
	private List<RichTextLabel> labels;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		labels = GetNode<Control>("TextBorders").GetChildren()
			.Cast<RichTextLabel>()
			.ToList();
		labels[0].Show();
		var tree = GetTree();
		tree.Root.GetNode<UI>("UI").HideHornButtons("r");
		tree.Root.GetNode<UI>("UI").ShowHornButtons("gb");
	}

	public void _on_pressed()
	{
		labels[0].Hide();
		labels.RemoveAt(0);
		if (!labels.Any())
		{
			var tree = GetTree();
			tree.ChangeSceneToFile("res://scenes/home/home.tscn");
			tree.Root.GetNode<Control>("UI").Show();
		}
		else
			labels[0].Show();
	}
}
