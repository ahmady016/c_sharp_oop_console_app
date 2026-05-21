using System.Text;
using System.Text.Json;

public static class Helpers
{
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
    public static async Task<List<Dictionary<string, object>>> ReadJsonFile(string filePath)
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

}
