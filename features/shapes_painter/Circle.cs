using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;
using Bogus;

namespace ShapesPainter;

// ── Circle ───────────────────────────────────────────────────────────────
public sealed class Circle : Shape
{
    private static readonly Faker _faker = new();

    private readonly double _radius;
    public Circle(
        double radius,
        Color color,
        float penThickness = 2f
    ) : base("Circle", 0, radius * 2, color, penThickness)
    {
        _radius = radius;
    }
    public override double Area() => Math.PI * Math.Pow(_radius, 2);
    public override double Perimeter() => 2 * Math.PI * _radius;

    [SupportedOSPlatform("windows")]
    protected override void PaintOnGraphics(Graphics g)
    {
        float r  = (float)_radius;
        float x  = CenterX - r;
        float y  = CenterY - r;
        float d  = r * 2;

        var shapeColor = Color.FromName(ShapeColor);
        using var fill = new SolidBrush(Color.FromArgb(80, shapeColor));
        using var pen  = new Pen(shapeColor, PenThickness);

        g.FillEllipse(fill, x, y, d, d);
        g.DrawEllipse(pen,  x, y, d, d);

        // center dot
        g.FillEllipse(new SolidBrush(shapeColor), CenterX - 3, CenterY - 3, 6, 6);

        // radius line
        using var dashPen = new Pen(Color.FromArgb(150, shapeColor), 1f)
                            { DashStyle = DashStyle.Dash };
        g.DrawLine(dashPen, CenterX, CenterY, CenterX + r, CenterY);
    }

    public static Circle CreateRandom() => new(
        radius: _faker.Random.Int(180, 360),
        color: Helpers.GetRandomColor(),
        penThickness: _faker.Random.Float(1.25f, 4.25f)
    );
}
