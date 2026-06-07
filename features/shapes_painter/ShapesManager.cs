/*
╔══════════════════════════════════════════════════════════════════════════╗
║                           SHAPES PAINTER APP                               ║
║                demonstrating OOP inheritance with                          ║
║        System.Drawing, bitmap rendering and saving as png images           ║
║  Abstract base: Shape, RegularPolygon                                      ║
║  Concrete Children: Triangle, Square, Rectangle,                           ║
║            Pentagon, Hexagon, Heptagon, Octagon,                           ║
║            Circle, Oval                                                    ║
╚══════════════════════════════════════════════════════════════════════════╝
// Full inheritance hierarchy
// ——————————————————————————
Shape -> abstract base define all shared
    fields -> (name, sides, sideSize, color, penThickness)
    properties -> GetPoints — used in drawing all of the shapes except (circle, oval)
    and operations -> (Area/Perimeter/Draw)
    ├── RegularPolygon abstract intermediate level
        ├── Triangle 3 sides — circumradius formula
        ├── Pentagon 5 sides
        ├── Hexagon 6 sides
        ├── Heptagon 7 sides
        ├── Octagon 8 sides
    ├── Square 4 sides — (width) — implement its own Area/Perimeter
    ├── Rectangle 4 sides (width, height) — implement its own Area/Perimeter
    ├── Circle 0 sides curved — implement its own Area/Perimeter/Draw
    ├── Oval 0 sides curved — implement its own Area/Perimeter/Draw
// ————————————————————————————————————————————————————————————————————————————————
// OOP concepts in play
// ————————————————————
Abstraction     ->  Shape and RegularPolygon are abstract — they define contracts, not concrete instances
Encapsulation   ->  Drawing internals hidden inside each class; callers only see Draw(), Area(), Perimeter()
Inheritance     ->  3-level chain: Shape → RegularPolygon → {Triangle, Pentagon...}
Override        ->  Circle and Oval override Draw(); Square/Rectangle override GetPoints()
Polymorphism    ->  List<Shape> holds all 9 types; shape.Draw() dispatches the right render per type
// ————————————————————————————————————————————————————————————————————————————————
// Architecture decisions worth studying
// —————————————————————————————————————
1. Three-level hierarchy on purpose:
    is not over-engineering it eliminates real code duplication. circumradius formula,
    vertex-generation algorithm, Area(), and Perimeter() are all written once in RegularPolygon.
    Adding a Nonagon (9 sides) or Decagon (10 sides) is literally one class with one constructor call.
2. Abstract Shape and RegularPolygon are abstract:
    They define contracts, not concrete instances.
    Drawing internals hidden inside each class; callers only see Draw(), Area(), Perimeter().
3. Template method pattern inside Draw() Method:
    The base Draw() owns the full pipeline — bitmap, anti-aliasing, labelling, saving.
    It calls PaintOnGraphics(g) which is virtual.
    Polygon shapes override GetPoints() and let the default PaintOnGraphics handle fill + stroke.
    Circle and Oval override PaintOnGraphics directly because they have no points
    and they use g.DrawEllipse. Neither path breaks the pipeline.
4. Square and Rectangle are direct (Shape) children not (RegularPolygon) children:
    A square has 4 equal sides but its area formula (s²) and its point generation (simple corner math)
    are simpler and more readable written directly no circumradius needed.
    A rectangle has unequal sides so it can never be a RegularPolygon at all.
5. Polymorphism when using List<Shape> holds all 9 types:
    shape.Draw() dispatches the right render per type and the right DrawOnGraphics per type.
    And the LINQ block runs statistical functions (MaxBy, MinBy, Sum, Average, and GroupBy)
    on the same List<Shape> all working against Area() and Perimeter()
    without knowing the concrete type.
*/
using System.Drawing;

namespace ShapesPainter;

public static class ShapesManager
{
    private static void PrintShapesStats(List<Shape> shapes)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  ── LINQ aggregations ");
        Console.WriteLine("  ──────────────────────────────────────────────");
        Console.ResetColor();

        var largest  = shapes.MaxBy(s => s.Area())!;
        var smallest = shapes.MinBy(s => s.Area())!;
        var totalArea= shapes.Sum(s => s.Area());
        var avgPerimeter = shapes.Average(s => s.Perimeter());

        Console.WriteLine($"  Largest  by area : {largest.Name}  ({largest.Area():F2})");
        Console.WriteLine($"  Smallest by area : {smallest.Name}  ({smallest.Area():F2})");
        Console.WriteLine($"  Total area       : {totalArea:F2}");
        Console.WriteLine($"  Avg perimeter    : {avgPerimeter:F2}");

        // group by type family
        var grouped = from s in shapes
                    let family = s is Circle or Oval
                        ? "Curved"
                        : s is Square or Rectangle or Triangle
                            ? "Quadrilateral/Triangle"
                            : "Polygon"
                    group s by family into g
                    select new { Family = g.Key,
                                Count  = g.Count(),
                                TotalArea = g.Sum(x => x.Area()) };

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  {"Family",-26} {"Count",5} {"Total Area",12}");
        Console.WriteLine($"  {new string('─', 46)}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Gray;
        foreach (var g in grouped)
            Console.WriteLine($"  {g.Family,-26} {g.Count,5} {g.TotalArea,12:F2}");
        Console.ResetColor();
    }

    public static void Run()
    {
        // ── print app header ──────────────────────────────────────────
        Helpers.PrintHeader("Shapes Painter App Started ...");

        // ── build one shape from each type of shapes ─────────────────────────────────────────────
        List<Shape> shapes =
        [
            new Triangle (sideSize: 150, color: Color.CornflowerBlue,           penThickness: 2.5f),
            new Square   (sideSize: 130, color: Color.MediumSeaGreen,           penThickness: 2f),
            new Rectangle(width: 200, height: 120, color: Color.Tomato,         penThickness: 2f),
            new Pentagon (sideSize: 100, color: Color.MediumOrchid,             penThickness: 2f),
            new Hexagon  (sideSize:  90, color: Color.DarkGoldenrod,            penThickness: 2f),
            new Heptagon (sideSize:  85, color: Color.SteelBlue,                penThickness: 2f),
            new Octagon  (sideSize:  80, color: Color.IndianRed,                penThickness: 2f),
            new Circle   (radius:   130, color: Color.DarkCyan,                 penThickness: 2.5f),
            new Oval     (radiusX: 150, radiusY: 90, color: Color.DarkMagenta,  penThickness: 2f),
        ];

        // ── print summary table header ──────────────────────────────────────────
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  {"Shape",-18} {"Sides",10} {"SideSize",10} {"Area",10} {"Perimeter",10}");
        Console.WriteLine($"  {new string('─', 70)}");
        Console.ResetColor();

        // ── print each shape summary ──────────────────────────────────────────
        Console.ForegroundColor = ConsoleColor.Gray;
        foreach (var shape in shapes)
            Console.WriteLine($" {shape.Name,-18} {shape.Sides,10} {shape.SideSize,10:F0} {shape.Area(),10:F2} {shape.Perimeter(),10:F2}");
        Console.WriteLine($"  {new string('─', 70)}");
        Console.ResetColor();

        // ── polymorphic Draw() call — each shape renders itself ──────────
        if (OperatingSystem.IsWindows())
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Drawing shapes → saving PNGs to ./images/");
            Console.ResetColor();
            Console.WriteLine();
            foreach (var shape in shapes)
                shape.Draw();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Skipping drawing: Shape.Draw() is supported only on Windows.");
            Console.ResetColor();
            Console.WriteLine();
        }

        // ── print all shapes stats ──────────────────────────────────────────
        PrintShapesStats(shapes);

        // ── print app footer ──────────────────────────────────────────
        Helpers.PrintFooter("Shapes Painter App Completed ...");
    }

}
