using System.Text.Json;
using System.Text.Json.Serialization;

namespace HotelManagement;

public class GuestIdJsonConverter : JsonConverter<Guest>
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
    private readonly string _rawGuestsJson;
    private readonly Dictionary<string, Guest> _guestsMap;
    public GuestIdJsonConverter()
    {
        string DATA_DIRECTORY = Path.Combine(Helpers.SameDirectory(), "data");
        string filePath = Path.Combine(DATA_DIRECTORY, "guests.json");
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Guests File Not Found.");
        _rawGuestsJson = File.ReadAllText(filePath);
        _guestsMap = JsonSerializer.Deserialize<List<Guest>>(_rawGuestsJson, _options)?.ToDictionary(g => g.Id) ?? [];
    }
    public override Guest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Console.WriteLine("Reading Guest Id...");
        string id = reader.GetString() ?? throw new JsonException("guestId cannot be null or empty.");
        Console.WriteLine("Guest Id Read...");
        return _guestsMap.TryGetValue(id, out Guest? guest)
            ? guest
            : throw new JsonException($"Guest with id ({id}) not found in the guests file.");
    }
    public override void Write(Utf8JsonWriter writer, Guest value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Id);
    }
}
