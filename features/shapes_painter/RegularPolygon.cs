using System.Drawing;

namespace ShapesPainter;

public abstract class RegularPolygon : Shape
{
    protected RegularPolygon(
        string name,
        int sides,
        double sideSize,
        Color? color,
        float penThickness
    ) : base(name, sides, sideSize, color, penThickness) { }

    public override double Perimeter() => Sides * SideSize;
    public override double Area() =>
        Sides * Math.Pow(SideSize, 2) / (4 * Math.Tan(Math.PI / Sides));

    // calculation the circumradius of a regular polygon
    // used to get the drawing points of the polygon
    protected double Circumradius => SideSize / (2 * Math.Sin(Math.PI / Sides));
    // vertices equally spaced around the center
    // rotated so one flat side sits at the top for polygons with an even number of sides
    protected override PointF[] GetPoints()
    {
        double r = Circumradius;
        // start from top and flat-top for even sides
        double offset = -Math.PI / 2;
        if (Sides % 2 == 0)
            offset += Math.PI / Sides;
        // return the points array
        return [..
            from _ in Enumerable.Range(0, Sides)
            let angle = offset + (2 * Math.PI * _ / Sides)
            select new PointF(
                (float)(CenterX + r * Math.Cos(angle)),
                (float)(CenterY + r * Math.Sin(angle))
            )
        ];
    }

}
