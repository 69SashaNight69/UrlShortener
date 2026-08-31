namespace UrlShortener.Api.DTOs;

public record ShortUrlDto(
    int Id,
    string OriginalUrl,
    string ShortCode,
    string CreatedBy,
    DateTime CreatedDate,
    bool CanDelete
 );