namespace UrlShortener.Api.Models;

public class ShortUrl
{
    public int Id { get; set; }

    public required string OriginalUrl { get; set; }

    public string? ShortCode { get; set; }

    public DateTime CreatedDate { get; set; }

    public required string CreatedByUserId { get; set; }

    public AppUser? CreatedByUser { get; set; }
}
