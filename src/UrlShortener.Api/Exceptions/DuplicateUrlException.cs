namespace UrlShortener.Api.Exceptions;

public class DuplicateUrlException : Exception
{
    public DuplicateUrlException(string originalUrl)
        : base($"URL '{originalUrl}' already exists.")
    {
    }
}