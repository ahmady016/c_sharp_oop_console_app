using System.Drawing;
using Bogus;

namespace ShapesPainter;

// ── Hexagon (6 sides) ────────────────────────────────────────────────────
public sealed class Hexagon : RegularPolygon
{
    private static readonly Faker _faker = new();
    public Hexagon(
        double sideSize,
        Color color,
        float penThickness = 2f
    ) : base("Hexagon", 6, sideSize, color, penThickness) { }

    public static Hexagon CreateRandom() => new(
        sideSize: _faker.Random.Int(180, 360),
        penThickness: _faker.Random.Float(1.25f, 4.25f),
        color: Helpers.GetRandomColor()
    );
}
