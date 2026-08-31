namespace UrlShortener.Api.Exceptions;

public class ShortUrlNotFoundException : Exception
{
    public ShortUrlNotFoundException() : base("Short URL not found.")
    {
    }
}