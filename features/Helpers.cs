using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;

public static class Helpers
{
    // json serialization global options
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
        },
    };
    // method to generate random unique id with the given length
    // using Base62/Base36 Character Pool
    public static string GenerateId(int length = 8)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be greater than 0.");

        const string CHARACTERS = "ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
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

    // helper method to get the file path in the same directory as the caller
    // the compiler replaces callerPath with the full path of the caller file at compile time
    // so SameDirectory("books.json") always resolves to the directory of the caller file
    // regardless of where the code is executed (Base Directory, bin folder, etc.)
    // or from or the current working directory
    public static string SameDirectory([CallerFilePath] string callerPath = "") =>
        Path.GetDirectoryName(callerPath)!;
    public static string PathWithinSameDirectory(
        string fileName, [CallerFilePath] string callerPath = ""
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

}
