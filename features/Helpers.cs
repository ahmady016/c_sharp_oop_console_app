using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Net.Mail;
using System.Drawing;
using Bogus;

public static partial class Helpers
{
    // Bogus Faker instance
    private static readonly Faker _faker = new();
    // string of all digits
    private static readonly string _digits = "0123456789";
    // string array of common web colors names
    public static readonly Color[] _webColors = [
        // Blues
        Color.CornflowerBlue, Color.SteelBlue, Color.DodgerBlue,
        Color.RoyalBlue, Color.DeepSkyBlue, Color.CadetBlue,
        Color.DarkCyan, Color.LightSeaGreen, Color.MediumTurquoise,
        // Greens
        Color.MediumSeaGreen, Color.SeaGreen, Color.ForestGreen,
        Color.OliveDrab, Color.LimeGreen, Color.YellowGreen,
        // Reds & Pinks
        Color.Tomato, Color.IndianRed, Color.Crimson, Color.HotPink, Color.MediumVioletRed,
        // Purples
        Color.MediumOrchid, Color.MediumPurple, Color.BlueViolet,
        Color.DarkMagenta, Color.Purple, Color.RebeccaPurple,
        // Oranges & Browns
        Color.DarkOrange, Color.OrangeRed, Color.DarkGoldenrod,
        Color.Chocolate, Color.SaddleBrown, Color.Sienna,
        // Neutrals
        Color.DimGray, Color.SlateGray, Color.DarkSlateGray,
    ];
    // json serialization global options
    public static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // tells the serializer to silently skip any JSON property that
        // has no matching constructor parameter or settable property
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
        },
    };

    // method to generate random digits only phone number with the given length
    public static string GenerateDigitsOnlyPhoneNumber(int length = 11)
        => _faker.Phone.PhoneNumber(new string('#', length));
    // method to generate random digits id with the given length
    public static string GenerateDigitsOnlyId(int length = 8)
        => new([..from _ in Enumerable.Repeat('_', length) select PickOne(_digits)]);

    // method to generate random unique characters id with the given length
    // using Base62/Base36 Character Pool
    public static string GenerateId(int length = 8)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be greater than 0.");

        const string CHARACTERS = "ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz123456789";
        StringBuilder id = new(length);
        for (int i = 0; i < length; i++)
            id.Append(CHARACTERS[Random.Shared.Next(CHARACTERS.Length)]);
        return id.ToString();
    }

    // method to generating CSS/web-compatible hex color string v1
    public static string GenerateHexColorV1()
    {
        // generate an integer value from 0 up to 0xFFFFFF (inclusive)
        int randomValue = Random.Shared.Next(0, 0x1000000);
        // format with a hashtag prefix and 6-digit hex notation
        return $"#{randomValue:X6}";
    }
    // method to generating CSS/web-compatible hex color string v2
    // using individual color channels (RGB)
    public static string GenerateHexColorV2()
    {
        var random = Random.Shared;
        // Generate 0-255 for each channel
        byte r = (byte)random.Next(0, 256);
        byte g = (byte)random.Next(0, 256);
        byte b = (byte)random.Next(0, 256);
        // format with a hashtag prefix and 6-digit hex notation
        return $"#{r:X2}{g:X2}{b:X2}";
    }
    // method to generate a random color of common web named colors
    public static Color GetRandomColor() => PickOne(_webColors);

    // helper method to get the file path in the same directory as the caller
    // the compiler replaces callerPath with the full path of the caller file at compile time
    // so SameDirectory("books.json") always resolves to the directory of the caller file
    // regardless of where the code is executed (Base Directory, bin folder, etc.)
    // or from or the current working directory
    public static string SameDirectory([CallerFilePath] string callerPath = "")
        => Path.GetDirectoryName(callerPath)!;
    public static string PathWithinSameDirectory(
        string fileName,
        [CallerFilePath] string callerPath = ""
    ) => Path.Combine(Path.GetDirectoryName(callerPath)!, fileName);

    // method to get the value based on json value kind
    // and if the value kind is an array, recursively parse the array elements
    // finally return the value as an object
    private static object ParseArray(JsonElement element) =>
        (string[])[..
            from value in element.EnumerateArray()
            select ParseElement(value).ToString()
        ];
    private static object ParseElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Null => string.Empty,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Array => ParseArray(element),
            _ => element.GetRawText()
        };
    }
    // method to read and deserialize JSON data from a file
    // using System.Text.Json and async I/O
    public static async Task<List<Dictionary<string, object>>> ReadJsonFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath.Trim()))
            throw new ArgumentNullException(nameof(filePath), $"Parameter [{nameof(filePath)}] is null or empty.");
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File does not exist.");
        try
        {
            // read file contents
            var json = await File.ReadAllTextAsync(filePath);
            // check if file is empty or contains only whitespace
            if (string.IsNullOrEmpty(json.Trim()))
                throw new ArgumentException("File is empty.");

            // deserialize into a list of dictionaries
            // where each dictionary represents single JSON Object
            var jsonList = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json) ?? [];

            // convert JsonElement values to string for easier processing
            return [..
                from item in jsonList
                select new Dictionary<string, object>(
                    from pair in item
                    select KeyValuePair.Create(pair.Key, ParseElement(pair.Value))
                )
            ];
        }
        catch (Exception)
        {
            throw;
        }
    }

    // method to serialize and write JSON data to a file
    public static async Task WriteToJsonFileAsync<T>(string filePath, T data)
    {
        // check for valid file path and data otherwise throw exceptions
        ArgumentException.ThrowIfNullOrEmpty(filePath.Trim(), nameof(filePath));
        ArgumentNullException.ThrowIfNull(data, nameof(data));
        // first create the file stream with .tmp file to write data to it
        string tempPath = $"{filePath}.tmp";
        // second writes directly to disk — no intermediate string allocation in memory
        await using (var fileStream = File.Create(tempPath))
            await JsonSerializer.SerializeAsync(fileStream, data, _jsonOptions);
        // finally after write success, move and overwrite the data to original path
        File.Move(tempPath, filePath, overwrite: true);
    }
    // method to read and deserialize JSON data from a file
    public static async Task<T> ReadFromJsonFileAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Guests File Not Found.");
        await using var fileStream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<T>(fileStream, _jsonOptions)
            ?? throw new InvalidOperationException("Unable to deserialize JSON file.");
    }
    // method to list the full paths of all JSON files in the specified directory
    public static List<string> GetJsonFilesPaths(
        string directoryPath,
        bool searchSubdirectories = false
    )
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            return [];
        var searchOption = searchSubdirectories
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;
        try { return [..Directory.EnumerateFiles(directoryPath, "*.json", searchOption)]; }
        catch (UnauthorizedAccessException) { return []; }
    }

    // method to randomly pick one value from a collection
    public static T PickOne<T>(IEnumerable<T> collection) =>
        collection.ElementAt(Random.Shared.Next(collection.Count()));
    // method to randomly pick many values from a collection with possible duplicates
    public static IEnumerable<T> PickMany<T>(IEnumerable<T> collection, int count) =>
        [..from _ in Enumerable.Range(0, count) select PickOne(collection)];
    // method to randomly pick multiple values from a collection without duplicates
    public static IEnumerable<T> PickSet<T>(IEnumerable<T> collection, int count)
    {
        HashSet<T> picked = [];
        while (picked.Count < count)
            picked.Add(PickOne(collection));
        return picked;
    }
    // method to randomly shuffle a collection using Fisher-Yates algorithm
    public static IEnumerable<T> Shuffle<T>(IEnumerable<T> collection)
    {
        List<T> list = [..collection];
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    // method to get the plural form of a word based on the count
    public static string GetPluralString(string value, int count) => $"{value}{(count > 1 ? "s" : "")}";

    // method to validate email using built in .NET MailAddress class
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email?.Trim()))
            return false;
        return MailAddress.TryCreate(email.Trim(), out _);
    }

    // method to validate an absolute URL using built in Uri class
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url?.Trim()))
            return false;
        return Uri.IsWellFormedUriString(url.Trim(), UriKind.Absolute);
    }

    // method to remove common formatting characters before validating phone number
    public static string CleanPhoneNumber(string phoneNumber) =>
        phoneNumber.Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace(".", "")
            .Replace("/", "")
            .Replace("+", "")
            .Replace("(", "")
            .Replace(")", "");
    // method to validate phone number (simple regex for international formats)
    [GeneratedRegex(@"^\+?[0-9]\d{7,14}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex _validMobileRegex();
    public static bool IsValidMobileNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber?.Trim()))
            return false;
        return _validMobileRegex().IsMatch(phoneNumber);
    }

    #region Console UI Helpers
    public static ConsoleColor GetRandomConsoleColor() =>
        PickOne([
            ConsoleColor.Gray,
            ConsoleColor.Green,
            ConsoleColor.Blue,
            ConsoleColor.Cyan,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
            ConsoleColor.White,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkGray,
        ]);
    public static void PrintHeader(string title)
    {
        if(string.IsNullOrEmpty(title.Trim()))
            throw new ArgumentNullException(nameof(title), $"Parameter [{nameof(title)}] is null or empty.");

        int width = Console.WindowWidth - 2;
        string border = new('═', width);
        int paddingWidth = (width - title.Length) / 2;
        string padding = new(' ', paddingWidth);

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"╔{border}╗");
        Console.WriteLine($"║{padding}{title}{padding}║");
        Console.WriteLine($"╚{border}╝");
        Console.ResetColor();
        Console.WriteLine();
    }
    public static void PrintFooter(string title)
    {
        if (string.IsNullOrEmpty(title.Trim()))
            throw new ArgumentNullException(nameof(title), $"Parameter [{nameof(title)}] is null or empty.");

        string footerTitle = $"{title} Completed Successfully.";
        int width = Console.WindowWidth - 2;
        string border = new('═', width);
        int paddingWidth = (width - footerTitle.Length) / 2;
        string padding = new(' ', paddingWidth);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {border}");
        Console.WriteLine($"  {padding}{footerTitle}{padding}");
        Console.WriteLine($"  {border}");
        Console.ResetColor();
    }
    public static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"  ──── {title} ──────");
        Console.ResetColor();
    }
    public static void RunScenario(string title, Action scenario)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("══════════════════════════════════════════════════════");
        Console.WriteLine($" ▶ Running {title} Scenario:");
        Console.WriteLine("══════════════════════════════════════════════════════");
        Console.ResetColor();
        scenario();
    }
    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {message}");
        Console.ResetColor();
    }
    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }
    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }
    #endregion

}
