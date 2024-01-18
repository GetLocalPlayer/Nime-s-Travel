using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class scene : Button
{
	[Export] string[] introLines;
	[Export] string NextScenePath;
	
	List<string> lines;
	RichTextLabel label;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var tree = GetTree();

		var ui = tree.Root.GetNode<UI>("UI");
		ui.HideHornButtons("r");
		ui.ShowHornButtons("gb");
		ui.Hide();

		var escMenu = tree.Root.GetNode<EscMenu>("EscMenu");
		escMenu.Hide();
		escMenu.Buttons["Continue"].GetParent<Control>().Show();
		escMenu.Buttons["NewGame"].GetParent<Control>().Hide();
        escMenu.EscapePressed += () =>
            escMenu.Visible = !escMenu.Visible;
        escMenu.VisibilityChanged += () =>
        {
            var processMode = escMenu.Visible ? ProcessModeEnum.Disabled : ProcessModeEnum.Always;
            tree.CurrentScene.ProcessMode = processMode;
            ui.ProcessMode = processMode;
        };
		
		label = GetNode<RichTextLabel>("RichTextLabel");
		label.Text = introLines[0];
		lines = new List<string>(introLines.Skip(1));
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton btn && btn.IsPressed() && !btn.IsEcho() && btn.ButtonIndex == MouseButton.Left)
		{
			if (lines.Any())
			{
				label.Text = lines[0];
				lines.RemoveAt(0);
			}
			else
			{
				var tree = GetTree();
				tree.Root.GetNode<Control>("UI").Show();
				var escMenu = tree.Root.GetNode<Control>("EscMenu");
				var buttonContainer = escMenu.GetNode<Control>("ButtonContainer");
        		buttonContainer.Position = buttonContainer.Position with {Y = buttonContainer.Position.Y - 25};
        		var buttonsBackdrop = escMenu.GetNode<Control>("ButtonsBackdrop");
        		buttonsBackdrop.Position = buttonsBackdrop.Position with {Y = buttonsBackdrop.Position.Y - 25};
				tree.ChangeSceneToFile(NextScenePath);
			}	
		}
    }
}
