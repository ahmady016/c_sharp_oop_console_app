using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;

namespace ShapesPainter;

/// <summary>
/// abstract base class for all shapes.
/// defines the shared contract: fields + operations.
/// </summary>
public abstract class Shape
{
    // private shared readonly fields
    private readonly string _name;
    private readonly int _sides;
    private readonly double _sideSize; // in pixels
    private readonly Color _color;
    private readonly float _penThickness;

    // public shared getter only properties
    public string Name => _name;
    public int Sides => _sides;
    public double SideSize => _sideSize;
    public Color ShapeColor => _color;
    public float PenThickness => _penThickness;

    // parameterized constructor to initialize the readonly fields
    // and do the input validation to insure constructing a valid shape
    protected Shape(
        string name,
        int sides,
        double sideSize,
        Color? color = null,
        float penThickness = 2
    )
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name), "Name cannot be null or empty.");
        if(sides < 0 || sides > 10)
            throw new ArgumentOutOfRangeException(nameof(sides), "Sides must be between 0 and 10.");
        if(sideSize <= 0 || sideSize > 1000)
            throw new ArgumentOutOfRangeException(nameof(sideSize), "Side size must be between 0 and 1000.");
        if(penThickness < 0.25 || penThickness > 10)
            throw new ArgumentOutOfRangeException(nameof(penThickness), "Pen thickness must be between 0.25 and 10.");

        _name = name;
        _sides = sides;
        _sideSize = sideSize;
        _color = color ?? Color.Black;
        _penThickness = penThickness;
    }

    // canvas dimensions (subclasses may override)
    protected virtual int CanvasWidth  => 400;
    protected virtual int CanvasHeight => 400;
    protected virtual int CenterX => CanvasWidth  / 2;
    protected virtual int CenterY => CanvasHeight / 2;

    /// <summary>
    /// returns a graphics-ready array of points for polygon drawing.
    /// each concrete shape subclass must override this.
    /// circles / Ovals override Draw() directly instead of using drawing Points.
    /// </summary>
    protected virtual PointF[] GetPoints() => [];

    // abstract operations
    public abstract double Area();
    public abstract double Perimeter();

    private readonly string DATA_DIRECTORY = Path.Combine(Helpers.SameDirectory(), "data");

    [SupportedOSPlatform("windows")]
    private void DrawLabel(Graphics g)
    {
        using var font   = new Font("Segoe UI", 9, FontStyle.Regular);
        using var bold   = new Font("Segoe UI", 11, FontStyle.Bold);
        using var brush  = new SolidBrush(Color.FromArgb(60, 60, 60));
        using var shadow = new SolidBrush(Color.FromArgb(30, 0, 0, 0));

        string title = Name;
        string info  = $"Sides: {Sides}   Side: {SideSize:F0}px";
        string area  = $"Area: {Area():F2}";
        string perimeter = $"Perimeter: {Perimeter():F2}";

        float y = CanvasHeight - 72f;
        g.DrawString(title, bold,  shadow, 11, y + 1);
        g.DrawString(title, bold,  brush,  10, y);
        g.DrawString(info,  font,  brush,  10, y + 18);
        g.DrawString(area,  font,  brush,  10, y + 34);
        g.DrawString(perimeter, font,  brush,  10, y + 50);
    }

    /// <summary>
    /// the actual core drawing logic. Overridden in Circle and Oval
    /// polygon shapes use the default implementation (GetPoints → FillPolygon + DrawPolygon).
    /// </summary>
    [SupportedOSPlatform("windows")]
    protected virtual void PaintOnGraphics(Graphics g)
    {
        var pts = GetPoints();
        if (pts.Length == 0) return;

        using var fill = new SolidBrush(Color.FromArgb(80, ShapeColor));
        using var pen  = new Pen(ShapeColor, PenThickness);

        pen.LineJoin = LineJoin.Round;

        g.FillPolygon(fill, pts);
        g.DrawPolygon(pen, pts);

        // draw vertices
        foreach (var p in pts)
            g.FillEllipse(new SolidBrush(ShapeColor), p.X - 3, p.Y - 3, 6, 6);
    }

    // ── concrete Draw — shared rendering pipeline ─────────────────────────
    /// <summary>
    /// Creates a Bitmap canvas, draws the shape, fills it, and saves to PNG.
    /// Subclasses override PaintOnGraphics for custom rendering (circle, oval).
    /// </summary>
    [SupportedOSPlatform("windows")]
    public virtual void Draw()
    {
        // check for windows operating system
        if(Environment.OSVersion.Platform != PlatformID.Win32NT)
            throw new PlatformNotSupportedException();

        // create the data directory if it doesn't exist
        if(!Directory.Exists(DATA_DIRECTORY))
            Directory.CreateDirectory(DATA_DIRECTORY);

        // create the canvas with high-quality rendering settings and white background
        using var bmp = new Bitmap(CanvasWidth, CanvasHeight);
        using var g   = Graphics.FromImage(bmp);
        g.SmoothingMode      = SmoothingMode.AntiAlias;
        g.InterpolationMode  = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode    = PixelOffsetMode.HighQuality;
        g.Clear(Color.White);

        // do the actual drawing
        PaintOnGraphics(g);

        // paint label: (name, area, perimeter)
        DrawLabel(g);

        // save to PNG
        string path = Path.Combine(DATA_DIRECTORY, $"{Name.Replace(" ", "_")}.png");
        bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);

        // console output
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"  ✓  {Name,-18} saved to → .\\data\\{Path.GetFileName(path)}");
        Console.ResetColor();
    }

    public override string ToString() =>
        $"{Name} | Sides: {Sides} | Side: {SideSize:F1}px | " +
        $"Color: {ShapeColor.Name} | Pen: {PenThickness}px | " +
        $"Area: {Area():F2} | Perimeter: {Perimeter():F2}";

}
