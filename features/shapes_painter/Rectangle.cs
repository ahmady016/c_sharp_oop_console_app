using System.Drawing;
using Bogus;

namespace ShapesPainter;

// ── Rectangle 4 sides (width × height) ──────────────────────────────────────────
public sealed class Rectangle : Shape
{
    private static readonly Faker _faker = new();
    private readonly double _width;
    private readonly double _height;
    public Rectangle(
        double width,
        double height,
        Color color,
        float penThickness = 2f
    ) : base("Rectangle", 4, width, color, penThickness)
    {
        _width  = width;
        _height = height;
    }
    public override double Area() => _width * _height;
    public override double Perimeter() => 2 * (_width + _height);
    protected override PointF[] GetPoints()
    {
        float w = (float)(_width  / 2);
        float h = (float)(_height / 2);
        return
        [
            new PointF(CenterX - w, CenterY - h),
            new PointF(CenterX + w, CenterY - h),
            new PointF(CenterX + w, CenterY + h),
            new PointF(CenterX - w, CenterY + h),
        ];
    }

    public static Rectangle CreateRandom() => new(
        width: _faker.Random.Int(180, 360),
        height: _faker.Random.Int(120, 240),
        penThickness: _faker.Random.Float(1.25f, 4.25f),
        color: Helpers.GetRandomColor()
    );
}
