using System.Drawing;

namespace ShapesPainter;

// ── Square (4 equal sides) ───────────────────────────────────────────────
public sealed class Square : Shape
{
    public Square(
        double sideSize,
        Color color,
        float penThickness = 2f
    ) : base("Square", 4, sideSize, color, penThickness) { }
    public override double Area() => SideSize * SideSize;
    public override double Perimeter() => 4 * SideSize;
    protected override PointF[] GetPoints()
    {
        float h = (float)(SideSize / 2);
        return
        [
            new PointF(CenterX - h, CenterY - h),
            new PointF(CenterX + h, CenterY - h),
            new PointF(CenterX + h, CenterY + h),
            new PointF(CenterX - h, CenterY + h),
        ];
    }

}
