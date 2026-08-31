using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Api.Models;

public class AppUser : IdentityUser
{
    public ICollection<ShortUrl> ShortUrls { get; set; } = new List<ShortUrl>();
}
