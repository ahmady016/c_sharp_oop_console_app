using System.Drawing;

namespace ShapesPainter;

// ── Pentagon (5 sides) ───────────────────────────────────────────────────
public sealed class Pentagon : RegularPolygon
{
    public Pentagon(
        double sideSize,
        Color? color,
        float penThickness
    ) : base("Pentagon", 3, sideSize, color, penThickness) { }
}
