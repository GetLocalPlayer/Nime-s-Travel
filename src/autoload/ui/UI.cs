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

	private Interactable currentInteractable;
	private Interactable reachedInteractable;
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
			reachedInteractable.OnClick();

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
					OnInteractionModalClick();
		};
	}

	private void OnInteractionModalClick()
	{
		if (interactionLines.Any())
		{
			interactionLabel.Text = interactionLines[0];
			interactionLines.RemoveAt(0);
			return;
		}

		interactionLabel.Hide();
		interactionModal.Hide();
		var i = currentInteractable;
		currentInteractable = null;
		if (inspectionImage.Visible)
		{			
			inspectionImage.Hide();
			i.OnInspectionFinished();
		}
		else
			i.OnInteractionFinished();
		
	}

	/* Вызываются из класса Nime как GetTree().CallGroup("UI", ...) */

	public void InteractableReached(Interactable i)
	{
		reachedInteractable = i;
		interactableHint.Hide();
		interactableButton.Icon = i.UIIcon;
		interactableButton.Text = i.UILabel;
		interactableButton.Show();
		hornContainer.Show();
		hornContainer.GetNode<AnimationPlayer>("Buttons/MagicSpark/AnimationPlayer").Play("RESET");
	}

	public void InteractableLeft(Interactable i)
	{
		if (reachedInteractable != i) return;
		reachedInteractable = null;
		interactableButton.Hide();
		hornContainer.Hide();
	}

	/*
	Вызываются из класса Interactable как GetTree().CallGroup("UI", ...).
	CallGroup вызывает функцию с переданным именем у КАЖДОГО нода в группе 
	"UI", что значит внутри функций должна быть проверка если текущий this 
	это нужный мне UI. Но поскольку группа "UI" содержит один единственный
	нод UI, я не заморачиваюсь.
	*/

	public void InteractableHovered(Interactable i)
	{
		interactableHint.Icon = i.UIIcon;
		interactableHint.Text = i.UILabel;
		interactableHint.Visible = !interactableButton.Visible;
	}

	public void InteractableUnhovered()
	{
		interactableHint.Icon = null;
		interactableHint.Text = null;
		interactableHint.Hide();
	}

	public void InteractableInspected(Interactable i)
	{
		currentInteractable = i;
		inspectionImage.Texture = i.InspectionTexture;
		inspectionImage.Show();
		if (!i.InspectionLines.IsEmpty())
		{
			interactionLines.AddRange(i.InspectionLines);
			interactionLines.RemoveAt(0);
			interactionLabel.Text = i.InspectionLines[0];
			interactionLabel.Show();
		}
		interactionModal.Show();
	}

	public void InteractableInitialInteraction(Interactable i)
	{
		currentInteractable = i;
		interactionLines.AddRange(i.InitialInteracitonLines);
		RunInteraction();
	}

	public void InteractableInteracted(Interactable i)
	{
		currentInteractable = i;
		interactionLines.AddRange(i.InteractionLines);
		RunInteraction();
	}
	private void RunInteraction()
	{
		interactionLabel.Text = interactionLines[0];
		interactionLabel.Show();
		interactionLines.RemoveAt(0);
		interactionModal.Show();
	}
}
