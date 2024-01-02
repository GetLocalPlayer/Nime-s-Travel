using Godot;
using System;
using System.Diagnostics.Metrics;

public partial class UI : Control
{
	private CenterContainer hornContainer;
	private Button interactableHint;
	private Button interactableButton;
	private RichTextLabel interactionText;

	private Interactable targetedInteractable;

	private int counter = 0;

	public override void _Ready()
	{
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
		{
			counter++;
			GD.Print($"Interactable button {counter}");
		};

		interactionText = mainContainer.GetNode<RichTextLabel>("InteractionText");
		interactionText.Text = null;
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
		targetedInteractable = i;
		interactableHint.Visible = false;
		interactableButton.Icon = i.UIIcon;
		interactableButton.Text = i.UILabel;
		interactableButton.Visible = true;
		hornContainer.Visible = true;
		hornContainer.GetNode<AnimationPlayer>("Buttons/MagicSpark/AnimationPlayer").Play("RESET");
	}

	public void InteractableLeft()
	{
		targetedInteractable = null;
		interactableButton.Visible = false;
		hornContainer.Visible = false;
	}
}
