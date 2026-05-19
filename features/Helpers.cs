using System.Text;

public static class Helpers
{
    // method to generate random unique id with the given length
    // using Base62/Base36 Character Pool
    public static string GenerateId(int length = 8)
    {
        if(length <= 0)
            throw new ArgumentException("Length must be greater than 0.");

        const string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder id = new(length);
        for(int i = 0; i < length; i++)
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

}
