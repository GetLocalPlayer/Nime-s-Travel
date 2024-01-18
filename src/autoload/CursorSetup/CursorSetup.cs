using System.Collections.Generic;
using Godot;

public partial class CursorSetup : Node
{
    public enum Shape : long
    {
        Default = Input.CursorShape.Arrow,
        Exit = Input.CursorShape.Move,
        Wtf = Input.CursorShape.PointingHand,
        Red = Input.CursorShape.Wait,
        Green = Input.CursorShape.Vsplit,
        Blue = Input.CursorShape.Busy,
    }

    Dictionary <Shape, Resource> resources = new();
    Dictionary <Shape, Vector2> hotspots = new();
    

    public override void _Ready()
    {
        var dir = "res://autoload/CursorSetup/";
        resources.Add(Shape.Default, ResourceLoader.Load($"{dir}default.png"));
        resources.Add(Shape.Exit, ResourceLoader.Load($"{dir}exit.png"));
        resources.Add(Shape.Wtf, ResourceLoader.Load($"{dir}wtf.png"));
        resources.Add(Shape.Red, ResourceLoader.Load($"{dir}default_red.png"));
        resources.Add(Shape.Green, ResourceLoader.Load($"{dir}default_green.png"));
        resources.Add(Shape.Blue, ResourceLoader.Load($"{dir}default_blue.png"));

        hotspots.Add(Shape.Default, new Vector2(0, 0));
        hotspots.Add(Shape.Exit, new Vector2(0, 7));
        hotspots.Add(Shape.Wtf, new Vector2(0, 7));
        hotspots.Add(Shape.Red, new Vector2(0, 0));
        hotspots.Add(Shape.Green, new Vector2(0, 0));
        hotspots.Add(Shape.Blue, new Vector2(0, 0));

        foreach (var entry in resources)
            Input.SetCustomMouseCursor(entry.Value, (Input.CursorShape)entry.Key, hotspots[entry.Key]);
    }

    public void SetCursorIcon(CursorSetup.Shape shape)
    {
        Input.SetDefaultCursorShape((Input.CursorShape)shape);
    }
}
