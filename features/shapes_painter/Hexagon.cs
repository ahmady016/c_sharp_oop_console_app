using System.Drawing;

namespace ShapesPainter;

// ── Hexagon (6 sides) ────────────────────────────────────────────────────
public sealed class Hexagon : RegularPolygon
{
    public Hexagon(
        double sideSize,
        Color color,
        float penThickness = 2f
    ) : base("Hexagon", 6, sideSize, color, penThickness) { }
}
