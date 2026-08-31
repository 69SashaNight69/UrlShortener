namespace UrlShortener.Api.DTOs;

public record LoginResponse(string Token, string UserName, string Role);