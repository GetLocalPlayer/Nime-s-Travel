using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

public partial class UI : Control
{
	private CenterContainer hornContainer;
	private Button interactableHint;
	private Button interactableButton;
	private RichTextLabel interactionLabel;
	private TextureRect inspectionImage;
	private Control interactionModal;

	private Interactable reachedInteractable;
	private List<string> inspectionLines = new List<string>();
	private List<string> interactionLines = new List<string>();

	public override void _Ready()
	{
		GetNode<CheckBox>("LanguageSwitcher").Toggled += (toggledOn) =>
			TranslationServer.SetLocale(toggledOn ? "ru" : "en");
		var mainContainer = GetNode<BoxContainer>("MainContainer");

		hornContainer = mainContainer.GetNode<CenterContainer>("HornContainer");
		hornContainer.Visible = false;

		var buttonsRoot = hornContainer.GetNode<Control>("Buttons");
		var animPlayer = buttonsRoot.GetNode<AnimationPlayer>("MagicSpark/AnimationPlayer");

		var pressedHandler = Callable.From((Button emitter) => 
		{
			animPlayer.Queue(emitter.Name);
			GetTree().CallGroup("Player", "HornButtonClicked", emitter.Name);
		});

		/* Сигнал кнопок "pressed" не предусметривает передачи
		в качестве параметра кнопку, которая была pressed. В
		GDScript мы можем использовать метод Callable.bind(...)
		чтобы привязать произвольные параметры к Callable который
		вызывается в момент излучения сигнала, но, похоже, этот
		метод все еще (версия движка 4.2.1) не реализован в С#
		версии API. Придется костылявить замыканиями. */
		var btnRed = buttonsRoot.GetNode<Button>("Red");
		btnRed.Pressed += () => pressedHandler.Call(btnRed);
		var btnGreen = buttonsRoot.GetNode<Button>("Green");
		btnGreen.Pressed += () => pressedHandler.Call(btnGreen);
		var btnBlue = buttonsRoot.GetNode<Button>("Blue");
		btnBlue.Pressed += () => pressedHandler.Call(btnBlue);

		var interactableContainer = mainContainer.GetNode<BoxContainer>("InteractableContainer");
		interactableHint = interactableContainer.GetNode<Button>("Hint");
		interactableHint.Visible = false;
		
		interactableButton = interactableContainer.GetNode<Button>("Button");
		interactableButton.Visible = false;
		interactableButton.Pressed += () =>
			InteractableClicked(reachedInteractable);

		interactionLabel = mainContainer.GetNode<RichTextLabel>("InteractionContainer/RichLabel");
		interactionLabel.Text = null;

		inspectionImage = GetNode<TextureRect>("InspectionImage");
		inspectionImage.Visible = false;

		interactionModal = GetNode<Control>("InteractionModal");
		interactionModal.Visible = false;
		interactionModal.GuiInput += (@event) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.Pressed && btn.ButtonIndex == MouseButton.Left)
				{
					if (inspectionLines.Any())
					{
						interactionLabel.Text = inspectionLines[0];
						inspectionLines.RemoveAt(0);
						return;
					}
					else if (inspectionImage.Visible)
					{
						inspectionImage.Visible = false;
						inspectionImage.Texture = null;
					}
					if (interactionLines.Any())
					{
						interactionLabel.Text = interactionLines[0];
						interactionLines.RemoveAt(0);
					}
					else
					{
						interactionModal.Visible = false;
						interactionLabel.Text = "";
					}
				}
		};
	}

	public void InteractableHovered(Interactable i)
	{
		interactableHint.Icon = i.UIIcon;
		interactableHint.Text = i.UILabel;
		interactableHint.Visible = !interactableButton.Visible;
	}

	public void InteractableUnhovered(Interactable i)
	{
		interactableHint.Icon = null;
		interactableHint.Text = null;
		interactableHint.Visible = false;
	}

	public void InteractableReached(Interactable i)
	{
		reachedInteractable = i;
		interactableHint.Visible = false;
		interactableButton.Icon = i.UIIcon;
		interactableButton.Text = i.UILabel;
		interactableButton.Visible = true;
		hornContainer.Visible = true;
		hornContainer.GetNode<AnimationPlayer>("Buttons/MagicSpark/AnimationPlayer").Play("RESET");
	}

	public void InteractableLeft()
	{
		reachedInteractable = null;
		interactableButton.Visible = false;
		hornContainer.Visible = false;
	}


	public void InteractableClicked(Interactable i)
	{
		if (reachedInteractable == i)
		{
			var (inspTexture,  inspLines) = reachedInteractable.GetInspectionData();
			if (inspTexture != null)
			{
				inspectionImage.Texture = inspTexture;
				inspectionImage.Visible = true;
				inspectionLines.AddRange(inspLines);
			}

			var interLines = reachedInteractable.GetInteractionLines();
			interactionLines.AddRange(interLines);

			if (inspectionLines.Any())
			{
				interactionLabel.Text = inspectionLines[0];
				inspectionLines.RemoveAt(0);
			}
			else
			{
				interactionLabel.Text = interactionLines[0];
				interactionLines.RemoveAt(0);
			}

			interactionModal.Visible = true;
			interactionLabel.Visible = true;
		}
	}
}
