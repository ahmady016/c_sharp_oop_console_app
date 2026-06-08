using System.Drawing;
using Bogus;

namespace ShapesPainter;

// ── Octagon (8 sides) ────────────────────────────────────────────────────
public sealed class Octagon : RegularPolygon
{
    private static readonly Faker _faker = new();
    public Octagon(
        double sideSize,
        Color color,
        float penThickness = 2f
    ) : base("Octagon", 8, sideSize, color, penThickness) { }

    public static Octagon CreateRandom() => new(
        sideSize: _faker.Random.Int(180, 360),
        penThickness: _faker.Random.Float(1.25f, 4.25f),
        color: Helpers.GetRandomColor()
    );
}
