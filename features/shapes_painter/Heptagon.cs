using System.Drawing;
using Bogus;

namespace ShapesPainter;

// ── Heptagon (7 sides) ───────────────────────────────────────────────────
public sealed class Heptagon : RegularPolygon
{
    private static readonly Faker _faker = new();
    public Heptagon(
        double sideSize,
        Color color,
        float penThickness = 2f
    ) : base("Heptagon", 7, sideSize, color, penThickness) { }

    public static Heptagon CreateRandom() => new(
        sideSize: _faker.Random.Int(180, 360),
        penThickness: _faker.Random.Float(1.25f, 4.25f),
        color: Helpers.GetRandomColor()
    );
}
