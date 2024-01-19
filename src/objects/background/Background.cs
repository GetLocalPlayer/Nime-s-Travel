using Godot;
using System;

public partial class Background : Sprite2D
{
	/* Background node that catches mousce clicks
	and calls "Player" group with the cursor's 
	global position to pave the way via
	NavigationRegion2D. */

	public override void _Ready()
	{	
		var zoneMusicPlayer = GetNode<AudioStreamPlayer>("ZoneMusicPlayer");

		var backgroundBusIdx = AudioServer.GetBusIndex("BackgroundMusic");
		var zoneBusIdx = AudioServer.GetBusIndex("ZoneMusic");

        void onTreeEntered()
        {
            if (zoneMusicPlayer.Autoplay && zoneMusicPlayer.Stream != null)
            {
                AudioServer.SetBusMute(backgroundBusIdx, true);
                AudioServer.SetBusMute(zoneBusIdx, false);
            }
        }

        TreeEntered += onTreeEntered;
		onTreeEntered();

		TreeExiting += () =>
		{
			if (zoneMusicPlayer.Autoplay && zoneMusicPlayer.Stream != null)
			{
				AudioServer.SetBusMute(backgroundBusIdx, false);
				AudioServer.SetBusMute(zoneBusIdx, true);
			}
		};

		var onInputEvent = Callable.From((Viewport v, InputEvent e, int idx) =>
		{
			if (v.IsInputHandled()) return;
			if (e is InputEventMouseButton btn && btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
				GetTree().CallGroup("Player", "BackgroundClicked", GetGlobalMousePosition());
		});

		/* I had to make signal handler deferred since the engine's physic
		system does not allow to arrange the order in which physical nodes
		react on click events. That was just not possible to predict which
		Area2D catches the mouse click first (that is applied only to Node2D
		inheriting classes, Control nodes' order works fine and predictable).
		So I delay Background node reaction on click event to "idle" frame
		to make sure that Interactable and SceneGate node catch and handle
		mouse events first marking them as "Handled". */
		var clickable = GetNode<Area2D>("Clickable");
		clickable.Connect(Area2D.SignalName.InputEvent, onInputEvent, (uint)ConnectFlags.Deferred);
	}	
}
