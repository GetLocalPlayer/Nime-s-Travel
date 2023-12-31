using Godot;
using System;
using System.Diagnostics.Metrics;

public partial class UI : Control
{
	private Interactable currentInterractable;
	private CenterContainer hornContainer;
	private TextureRect horn;
	private Button btnRed;
	private Button btnGreen;
	private Button btnBlue;
	private TextureRect interactableIcon;
	private Label interactableLabel;
	private RichTextLabel interactionText;

	private int counter = 0;
	public override void _Ready()
	{
		var mainContainer = GetNode<BoxContainer>("MainContainer");

		hornContainer = mainContainer.GetNode<CenterContainer>("HornContainer");
		hornContainer.Visible = false;
		horn = hornContainer.GetNode<TextureRect>("Horn");
		btnRed = hornContainer.GetNode<Button>("ButtonRed");
		btnGreen = hornContainer.GetNode<Button>("ButtonGreen");
		btnBlue = hornContainer.GetNode<Button>("ButtonBlue");

		var interactableContainer = mainContainer.GetNode<BoxContainer>("InteractableContainer");
		interactableIcon = interactableContainer.GetNode<TextureRect>("Icon");
		interactableIcon.Texture = null;
		interactableIcon.GuiInput += (@event) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
				{
					counter++;
					GD.Print($"Click {counter}");
				}
		};
		interactableLabel = interactableContainer.GetNode<Label>("Label");
		interactableLabel.Text = null;

		interactionText = mainContainer.GetNode<RichTextLabel>("InteractionText");
		interactionText.Text = null;
	}

	private void SetInteractable(Interactable i)
	{
		currentInterractable = i;
		interactableIcon.Texture = i.Icon;
		interactableLabel.Text = i.UIName;
	}

	private void RemoveInteractable(Interactable i)
	{
		if (currentInterractable == null || currentInterractable != i) return;
		currentInterractable = null;
		interactableIcon.Texture = null;
		interactableLabel.Text = "";
	}

	private void EnableHorn() => hornContainer.Visible = true;
	private void DisableHorn() => hornContainer.Visible = false;
}
