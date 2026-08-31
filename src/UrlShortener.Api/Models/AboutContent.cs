namespace UrlShortener.Api.Models;

public class AboutContent
{
    public int Id { get; set; }

    public required string Text { get; set; }

    public DateTime LastUpdated { get; set; }
}