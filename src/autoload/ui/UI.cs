using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

public partial class UI : Control
{
	[Signal] public delegate void InteractionFinishedEventHandler();
	[Signal] public delegate void SpellRevealedEventHandler();
	[Signal] public delegate void InteractableButtonClickedEventHandler();

	TextureRect horn;
	Button interactableHint;
	Button interactableButton;
	AnimationPlayer msAnimPlayer;

	RichTextLabel interactionLabel;
	TextureRect inspectionImage;

	/* Кнтрол на весь экран перехватывающий
	события мыши для перелистования текста. */
	Control interactionModal;
	/* То же самое что и кнтрол выше но
	для катсцен (демонастрации заклинаний,
	взлета колбы и прочего). */
	Control cutsceneModal;
	/* Хранит в себе Interactable к которому
	подошла Ниме, нужен для отображения 
	кнопки в нижнем-правом углу. */
	Interactable reachedInteractable;
	/* Пеоеменна для передачи между функциями
	Interactable с которым происходит
	взаимодействие. */
	Interactable currentInteractable;
	/* Строки текста взаимодействия, сменяющие
	друг друга при по клику мышия. Нужен только
	для передачи между вызовами
	OnInteractionModalClick(). */
	List<string> interactionLines = new List<string>();

	Dictionary<string, Button> hornButtons = new(StringComparer.OrdinalIgnoreCase);

	public override void _Ready()
	{
		var mainContainer = GetNode<BoxContainer>("MainContainer");

		horn = mainContainer.GetNode<TextureRect>("HornContainer/Horn");
		horn.Hide();

		var buttonsRoot = horn.GetNode<Control>("Buttons");
		msAnimPlayer = buttonsRoot.GetNode<AnimationPlayer>("MagicSpark/AnimationPlayer");


		var btnRed = buttonsRoot.GetNode<Button>("Red");
		var btnGreen = buttonsRoot.GetNode<Button>("Green");
		var btnBlue = buttonsRoot.GetNode<Button>("Blue");

		hornButtons.Add("r", btnRed);
		hornButtons.Add("g", btnGreen);
		hornButtons.Add("b", btnBlue);

		var btn2Code = new Dictionary<Button, char>{
			{btnRed, 'r'},
			{btnGreen, 'g'},
			{btnBlue, 'b'},
		};

		void pressedHandler(Button emitter)
		{
			msAnimPlayer.Queue(emitter.Name);
			GetTree().CallGroup("Player", "HornButtonClicked", btn2Code[emitter]);
		}

		btnRed.Pressed += () => pressedHandler(btnRed);
		btnGreen.Pressed += () => pressedHandler(btnGreen);
		btnBlue.Pressed += () => pressedHandler(btnBlue);

		btnRed.MouseDefaultCursorShape = (Control.CursorShape)CursorSetup.Shape.Red;
		btnGreen.MouseDefaultCursorShape = (Control.CursorShape)CursorSetup.Shape.Green;
		btnBlue.MouseDefaultCursorShape = (Control.CursorShape)CursorSetup.Shape.Blue;

		var interactableContainer = mainContainer.GetNode<BoxContainer>("InteractableContainer");
		interactableHint = interactableContainer.GetNode<Button>("Hint");
		interactableHint.Hide();
		
		interactableButton = interactableContainer.GetNode<Button>("Button");
		interactableButton.Hide();
		interactableButton.Pressed += () =>
			EmitSignal("InteractableButtonClicked");

		/* Заполнитель на место иконки в нижнем-правом углу, чтобы
		текс не уезжал до края в случае отсутствия отображаемой иконки.
		Ту же фигну проделываю с рогом. */
		var interactablePlaceholder = mainContainer.GetNode<VSeparator>("InteractablePlaceholder");
        void onVisibilityChanged() =>
            interactablePlaceholder.Visible = !interactableHint.Visible && !interactableButton.Visible;
        interactableButton.VisibilityChanged += onVisibilityChanged;
		interactableHint.VisibilityChanged += onVisibilityChanged;

		interactionLabel = mainContainer.GetNode<RichTextLabel>("InteractionContainer/RichLabel");
		interactionLabel.Text = null;

		inspectionImage = GetNode<TextureRect>("InspectionImage");
		inspectionImage.Hide();

		interactionModal = GetNode<Control>("InteractionModal");
		interactionModal.Hide();

		cutsceneModal = GetNode<Control>("CutsceneModal");
		cutsceneModal.Hide();

		void onModalVisibilityChange() =>
			Input.MouseMode = cutsceneModal.Visible || interactionModal.Visible ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
		interactionModal.VisibilityChanged += onModalVisibilityChange;
		cutsceneModal.VisibilityChanged += onModalVisibilityChange;
	}

	public void SetInteracitonText(string text)
	{
		interactionLabel.Text = text;
		interactionLabel.Show();
	}

	public void SetHint(CompressedTexture2D icon, string label)
	{
		interactableHint.Icon = icon;
		interactableHint.Text = label;
		interactableHint.Visible = !interactableButton.Visible;
	}

	public void ClearHint()
	{
		interactableHint.Icon = null;
		interactableHint.Text = "";
		interactableHint.Hide();
	}

	public void SetHintButton(CompressedTexture2D icon, string label)
	{
		interactableButton.Icon = icon;
		interactableButton.Text = label;
		interactableButton.Show();
		interactableHint.Hide();
	}

	public void ClearHintButton()
	{
		interactableButton.Icon = null;
		interactableButton.Text = "";
		interactableButton.Hide();
	}

	public void ShowHornButtons(string buttons)
	{
		foreach (var c in buttons)
		{
			var s = c.ToString();
			if (hornButtons.ContainsKey(s))
				hornButtons[s].Show();
		}
	}

	public void HideHornButtons(string buttons)
	{
		foreach (var c in buttons)
		{
			var s = c.ToString();
			if (hornButtons.ContainsKey(s))
				hornButtons[s].Hide();
		}
	}

	public void ShowHorn()
	{
		horn.Show();
	}

	public void HideHorn()
	{
		StopCast();
		horn.Hide();
	}

	public void BlockMouse()
	{
		cutsceneModal.Show();
	}

	public void UnblockMouse()
	{
		cutsceneModal.Hide();
	}

	public void StopCast()
	{
		var animPlayer = horn.GetNode<AnimationPlayer>("Buttons/MagicSpark/AnimationPlayer");
		animPlayer.ClearQueue();
		animPlayer.Play("RESET");
	}

	async public void RevealSpell(string spellName)
	{
		var spells = GetTree().Root.GetNode<Spells>("Spells");

		var spellCode = spells.GetSpellCode(spellName);
		if (spellCode == null)
			throw new Exception($"No spell code found for {spellName} during UI spell reveal.");

		msAnimPlayer.Stop();
		foreach (var c in spellCode)
		{
			var anim2Play = "";
			switch (c)
			{
				case 'r':
					anim2Play = "Red";
					break;
				case 'g':
					anim2Play = "Green";
					break;
				case 'b':
					anim2Play = "Blue";
					break;
			}
			msAnimPlayer.Queue(anim2Play);
		}
		cutsceneModal.Show();
		await ToSignal(msAnimPlayer, AnimationPlayer.SignalName.AnimationFinished);
		cutsceneModal.Hide();
		EmitSignal("SpellRevealed");
	}

	public void RunInteraction(string[] lines)
	{
		if (lines.IsEmpty()) return;

		var list = new List<string>(lines);
		interactionLabel.Text = list[0];
		list.RemoveAt(0);
		interactionLabel.Show();
		interactionModal.Show();

        void handler(InputEvent @event)
        {
            if (!(@event is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == MouseButton.Left))
                return;

            if (list.Any())
            {
                interactionLabel.Text = list[0];
                list.RemoveAt(0);
                return;
            }

            if (inspectionImage.Visible)
            {
                inspectionImage.Texture = null;
                inspectionImage.Hide();
            }

            interactionLabel.Hide();
            interactionModal.Hide();
            interactionModal.GuiInput -= handler;
            EmitSignal("InteractionFinished");
        }

        interactionModal.GuiInput += handler;
	}

	public void RunPicturedInteraction(CompressedTexture2D texture, string[] lines)
	{
        inspectionImage.Texture = texture ?? throw new Exception("Must provide a texture to run inspeciton!");
		inspectionImage.Show();
		RunInteraction(lines.IsEmpty() ? new string[1]{""} : lines);
	}
}
