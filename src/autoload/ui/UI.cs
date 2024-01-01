using Godot;
using System;
using System.Diagnostics.Metrics;

public partial class UI : Control
{
	private CenterContainer hornContainer;
	private TextureRect horn;
	private TextureRect interactableIcon;
	private Label interactableLabel;
	private RichTextLabel interactionText;

	private Interactable focusedInteractable;

	private int counter = 0;

	private delegate void PressedEventHandler();

	public override void _Ready()
	{
		var mainContainer = GetNode<BoxContainer>("MainContainer");

		hornContainer = mainContainer.GetNode<CenterContainer>("HornContainer");
		hornContainer.Visible = false;

		var buttonsRoot = hornContainer.GetNode<Control>("Buttons");
		var animPlayer = buttonsRoot.GetNode<AnimationPlayer>("AnimationPlayer");

		var pressedHandler = Callable.From((Button emitter) => 
		{
			animPlayer.Queue(emitter.Name);
		});

		/* Сигнал кнопок "pressed" не предусметривает передачи
		в качестве параметра кнопки, которая была pressed. В
		GDScript мы можем использовать метод Callable.bind(...)
		чтобы привязать произвольные параметры к Callable который
		вызывается в момент излучения сигнала, но, похожу, этот
		метод все еще (версия движка 4.2.1) не реализован в С#
		версии API. Придется костылявить замыканиями. */
		var btnRed = buttonsRoot.GetNode<Button>("Red");
		btnRed.Pressed += () => pressedHandler.Call(btnRed);
		var btnGreen = buttonsRoot.GetNode<Button>("Green");
		btnGreen.Pressed += () => pressedHandler.Call(btnGreen);
		var btnBlue = buttonsRoot.GetNode<Button>("Blue");
		btnBlue.Pressed += () => pressedHandler.Call(btnBlue);

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
		//interactionText.Text = null;
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
