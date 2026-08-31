using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
public class RedirectController : ControllerBase
{
    private readonly ShortUrlService _service;

    public RedirectController(ShortUrlService service)
    {
        _service = service;
    }

    [HttpGet("/{shortCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> RedirectToOriginal(string shortCode)
    {
        var originalUrl = await _service.GetOriginalUrlAsync(shortCode);

        if (originalUrl is null)
        {
            return NotFound();
        }

        return Redirect(originalUrl);
    }
}