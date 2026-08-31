namespace UrlShortener.Api.Services;

public static class Base62Encoder
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Encode(int value)
    {
        if (value == 0)
        {
            return Alphabet[0].ToString();
        }

        var chars = new Stack<char>();

        while (value > 0)
        {
            var remainder = value % 62;
            chars.Push(Alphabet[remainder]);
            value /= 62;
        }

        return new string(chars.ToArray());
    }
}