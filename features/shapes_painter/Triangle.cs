using System.Drawing;
using Bogus;

namespace ShapesPainter;

// ── Triangle (3 sides) ────────────────────────────────────────────────────
public sealed class Triangle : RegularPolygon
{
    private static readonly Faker _faker = new();
    public Triangle(
        double sideSize,
        Color? color,
        float penThickness
    ) : base("Triangle", 3, sideSize, color, penThickness) { }

    public static Triangle CreateRandom() => new(
        sideSize: _faker.Random.Int(180, 360),
        penThickness: _faker.Random.Float(1.25f, 4.25f),
        color: Helpers.GetRandomColor()
    );
}
