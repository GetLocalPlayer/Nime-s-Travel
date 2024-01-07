using Godot;
using System;

[Tool] public partial class PointDrawer : Marker2D
{
	[ExportGroup("Editor Marker")]
	private CompressedTexture2D _texture;
	[Export] private CompressedTexture2D Texture {
		get => _texture;
		set {
			_texture = value;
			if (IsNodeReady()) QueueRedraw();
		}
	}

	private Vector2 _offset;
	[Export] private Vector2 Offset {
		get => _offset;
		set {
			_offset = value;
			if (IsNodeReady()) QueueRedraw();
		}
	}

	private Vector2 _size;
	[Export] private Vector2 Size {
		get => _size;
		set {
			_size = value;
			if (IsNodeReady()) QueueRedraw();
		}
	}

	private bool _drawIcon = true;
	[Export] private bool DrawIcon {
		get => _drawIcon;
		set{
			_drawIcon = value;
			if (IsNodeReady()) QueueRedraw();
		}
	}

    public override void _Ready()
    {
        if (!Engine.IsEditorHint()) return;

		ItemRectChanged += () => QueueRedraw();
    }

    public override void _Draw()
	{
		if (!Engine.IsEditorHint()) return;
	
		if (Texture != null && DrawIcon)
			DrawTextureRect(Texture, new Rect2(Offset, Size), false);
	}
}
