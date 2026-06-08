using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;
using Bogus;

namespace ShapesPainter;

// ── Oval (ellipse) ───────────────────────────────────────────────────────
public sealed class Oval : Shape
{
    private static readonly Faker _faker = new();

    private readonly double _radiusX;
    private readonly double _radiusY;
    public Oval(
        double radiusX,
        double radiusY,
        Color color,
        float penThickness = 2f
    ) : base("Oval", 0, radiusX * 2, color, penThickness)
    {
        _radiusX = radiusX;
        _radiusY = radiusY;
    }
    // Ramanujan's approximation for ellipse perimeter
    public override double Perimeter()
    {
        double a = _radiusX;
        double b = _radiusY;
        double h = Math.Pow(a - b, 2) / Math.Pow(a + b, 2);
        return Math.PI * (a + b) * (1 + 3 * h / (10 + Math.Sqrt(4 - 3 * h)));
    }
    public override double Area() => Math.PI * _radiusX * _radiusY;

    [SupportedOSPlatform("windows")]
    protected override void PaintOnGraphics(Graphics g)
    {
        float rx = (float)_radiusX;
        float ry = (float)_radiusY;
        float x  = CenterX - rx;
        float y  = CenterY - ry;

        using var fill = new SolidBrush(Color.FromArgb(80, ShapeColor));
        using var pen  = new Pen(ShapeColor, PenThickness);

        g.FillEllipse(fill, x, y, rx * 2, ry * 2);
        g.DrawEllipse(pen,  x, y, rx * 2, ry * 2);

        // axes lines
        using var dashPen = new Pen(Color.FromArgb(150, ShapeColor), 1f)
                            { DashStyle = DashStyle.Dash };
        g.DrawLine(dashPen, CenterX - rx, CenterY, CenterX + rx, CenterY); // major
        g.DrawLine(dashPen, CenterX, CenterY - ry, CenterX, CenterY + ry); // minor
        g.FillEllipse(new SolidBrush(ShapeColor), CenterX - 3, CenterY - 3, 6, 6);
    }

    public static Oval CreateRandom() => new(
        radiusX: _faker.Random.Int(140, 360),
        radiusY: _faker.Random.Int(140, 360),
        penThickness: _faker.Random.Float(1.25f, 4.25f),
        color: Helpers.GetRandomColor()
    );
}
