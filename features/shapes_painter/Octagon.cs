using System.Drawing;

namespace ShapesPainter;

// ── Octagon (8 sides) ────────────────────────────────────────────────────
public sealed class Octagon : RegularPolygon
{
    public Octagon(
        double sideSize,
        Color color,
        float penThickness = 2f
    ) : base("Octagon", 8, sideSize, color, penThickness) { }
}
