using Godot;
using System;
using System.Diagnostics.Metrics;

public partial class UI : Control
{
	private CenterContainer hornContainer;
	private TextureRect horn;
	private Button btnRed;
	private Button btnGreen;
	private Button btnBlue;
	private TextureRect interactableIcon;
	private Label interactableLabel;
	private RichTextLabel interactionText;

	private Interactable focusedInteractable;

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

	private void SetInteractableHint(Interactable i)
	{
		if (focusedInteractable == null || focusedInteractable == i)
		{
			interactableIcon.Texture = i.Icon;
			interactableLabel.Text = i.UIName;
		}
	}

	private void RemoveInteractableHint()
	{
		if (focusedInteractable != null) return;
		interactableIcon.Texture = null;
		interactableLabel.Text = null;
	}

	private void SetInteractableFocused(Interactable i)
	{
		focusedInteractable = i;
		SetInteractableHint(i);
		hornContainer.Visible = true;
	}

	private void RemoveInteractableFocused()
	{
		focusedInteractable = null;
		RemoveInteractableHint();
		hornContainer.Visible = false;
	}

	private void EnableHorn() => hornContainer.Visible = true;
	private void DisableHorn() => hornContainer.Visible = false;
}
