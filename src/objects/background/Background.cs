using Godot;
using System;

public partial class Background : Sprite2D
{
	/* Задник отлавливает клик мышкой и передает
	группе игроков глобальную позицию курсора
	к которой строится маршрут по навигационной
	сетке. */

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

		/* Пришлось перенести реакцию на сигнал в отложенный (deferred)
		вызов поскольку в движке нет встроенной функции упорядочивания
		событий ввода или их приоритетов, что приводило к тому что
		Clickable ноды у задников и экземпляров Interactable конкурировали
		за один и тот же клик. В классе Interactable я, конечно, останавливал
		поток выполнения по событию ввода путем вызова функции SetInputAsHandled,
		что работало прекрасно, если движок фиксировал сначала клик по Interactable
		и уже потом по Background, но если порядок был обртаный, начиналась
		жопа, поскольку Ниме переходила в состояние "Walk" и затем в состояние
		"WalkToInteractable" в тот же самый кадр. */
		var clickable = GetNode<Area2D>("Clickable");
		clickable.Connect(Area2D.SignalName.InputEvent, onInputEvent, (uint)ConnectFlags.Deferred);
	}	
}
