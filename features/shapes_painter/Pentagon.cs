using System.Drawing;
using Bogus;

namespace ShapesPainter;

// ── Pentagon (5 sides) ───────────────────────────────────────────────────
public sealed class Pentagon : RegularPolygon
{
    private static readonly Faker _faker = new();
    public Pentagon(
        double sideSize,
        Color? color,
        float penThickness
    ) : base("Pentagon", 5, sideSize, color, penThickness) { }

    public static Pentagon CreateRandom() => new(
        sideSize: _faker.Random.Int(180, 360),
        penThickness: _faker.Random.Float(1.25f, 4.25f),
        color: Helpers.GetRandomColor()
    );
}
