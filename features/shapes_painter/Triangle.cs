using System.Drawing;

namespace ShapesPainter;

// ── Triangle (3 sides) ────────────────────────────────────────────────────
public sealed class Triangle : RegularPolygon
{
    public Triangle(
        double sideSize,
        Color? color,
        float penThickness
    ) : base("Triangle", 3, sideSize, color, penThickness) { }
}
