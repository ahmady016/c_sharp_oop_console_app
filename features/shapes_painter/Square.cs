using System.Drawing;
using Bogus;

namespace ShapesPainter;

// ── Square (4 equal sides) ───────────────────────────────────────────────
public sealed class Square : Shape
{
    private static readonly Faker _faker = new();
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

    public static Square CreateRandom() => new(
        sideSize: _faker.Random.Int(180, 360),
        penThickness: _faker.Random.Float(1.25f, 4.25f),
        color: Helpers.GetRandomColor()
    );
}
