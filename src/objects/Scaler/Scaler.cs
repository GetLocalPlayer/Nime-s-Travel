using Godot;
using System;


/* Нод, масштабирующий Ниме (для масштабирования
в момент передвижения по дороге)

-- Scaler        // Главная нода
---- MaxScale    // Максимальный масштаб
---- MinScale    // Минимальный масштаб


Нод каждый кадр интерполирует масштаб Ниме до
масштаба между маркерами MinScale и MaxScale
в зависимости от ее положения по оси Y.

В идеале следовало описать граф через GDNatives
для поиска путей и масштабирования вдоль граней
графа, но пока что только вот такой костыль и
NavigationRegion для поиска путей. */
public partial class Scaler : Node2D
{
    Marker2D max;
    Marker2D min;

    public override void _Ready()
    {
        max = GetNode<Marker2D>("MaxScale");
        min = GetNode<Marker2D>("MinScale");
        GetParent().ChildEnteredTree += (node) =>
        {
            if (node.IsInGroup("Player"))
                _Process(0);
        };
    }

    public override void _Process(double delta)
    {
        foreach (var node in GetTree().GetNodesInGroup("Player"))
        {
            var nime = node as Nime;
            var factor = (nime.Position.Y - min.Position.Y) / (max.Position.Y - min.Position.Y);
            nime.Scale = (min.Scale + (max.Scale - min.Scale) * factor) * nime.Scale.Sign();
        }
    }
}
