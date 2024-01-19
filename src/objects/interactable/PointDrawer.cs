using Godot;
using System;


/* I wrote this class only for the engines Editor, it
draws the given texture on Marker2D position with
a line to it from its parrent. It's used for SceneGate
and Interactable markers to simplify visual understanding
of their scenes. If there's several interactables/gates one
next to another it was difficult to visually understand
which Marked2D belongs to which interactable/gate. */

[Tool] public partial class PointDrawer : Marker2D
{
	[ExportGroup("Icon")]
	CompressedTexture2D _texture;
	[Export] CompressedTexture2D Texture {
		get => _texture;
		set { _texture = value; QueueRedraw(); }
	}

	private bool _drawIcon = true;
	[Export] private bool DrawIcon {
		get => _drawIcon;
		set { _drawIcon = value; QueueRedraw(); }
	}

	private Vector2 _offset;
	[Export] private Vector2 Offset {
		get => _offset;
		set { _offset = value; QueueRedraw(); }
	}

	private Vector2 _size;
	[Export] private Vector2 Size {
		get => _size;
		set { _size = value; QueueRedraw(); }
	}

	[ExportGroup("Line")]

	bool _drawLines;
	[Export] bool DrawLines {
		get => _drawLines;
		set { _drawLines = value; QueueRedraw(); }
	}

	int _lineWidth = 16;
	[Export] int LineWidth {
		get => _lineWidth;
		set { _lineWidth = value; QueueRedraw(); }
	}

	int _lineDash = 0;
	[Export] int LineDash {
		get => _lineDash;
		set { _lineDash = value; QueueRedraw(); }
	}

	Color _LineColor = new Color(1f, 1f, 1f, 0.5f);
	[Export] Color LineColor {
		get => _LineColor;
		set { _LineColor = value; QueueRedraw(); }
	}


	Vector2 _lineFromOffset = Vector2.Zero;
	[Export] Vector2 LineFromOffset {
		get => _lineFromOffset;
		set { _lineFromOffset = value; QueueRedraw(); }
	}

	Vector2 _lineToOffset = Vector2.Zero;
	[Export] Vector2 LineToOffset {
		get => _lineToOffset;
		set { _lineToOffset = value; QueueRedraw(); }
	}

    public override void _Ready()
    {
        if (!Engine.IsEditorHint()) return;

		ItemRectChanged += () => QueueRedraw();
		SetNotifyTransform(true);
		SetNotifyLocalTransform(true);
    }

    public override void _Draw()
	{
		if (!Engine.IsEditorHint()) return;
		if (!IsNodeReady()) return;
	
		if (DrawLines && GetParent() is Node2D)
			if (LineDash > 0)
				DrawDashedLine(ToLocal(GetParent<Node2D>().GlobalPosition) + LineFromOffset, LineToOffset, LineColor, LineWidth, LineDash);
			else
				DrawLine(ToLocal(GetParent<Node2D>().GlobalPosition) + LineFromOffset, LineToOffset, LineColor, LineWidth);
			

		if (Texture != null && DrawIcon)
			DrawTextureRect(Texture, new Rect2(Offset, Size), false);
	}

    public override void _Notification(int what)
    {
        if (!Engine.IsEditorHint()) return;

		if (what == NotificationTransformChanged)
			QueueRedraw();
    }
}
