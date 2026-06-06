using System.Drawing;

namespace ShapesPainter;

// ── Heptagon (7 sides) ───────────────────────────────────────────────────
public sealed class Heptagon : RegularPolygon
{
    public Heptagon(
        double sideSize,
        Color color,
        float penThickness = 2f
    ) : base("Heptagon", 7, sideSize, color, penThickness) { }
}
