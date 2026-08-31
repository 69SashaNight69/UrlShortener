using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrlShortener.Api.DTOs;
using UrlShortener.Api.Exceptions;
using UrlShortener.Api.Models;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShortUrlsController : ControllerBase
{
    private readonly ShortUrlService _service;

    public ShortUrlsController(ShortUrlService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ShortUrlDto>>> GetAll()
    {
        var result = await _service.GetAllAsync(CurrentUserId, IsAdmin);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ShortUrlDto>> GetById(int id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, CurrentUserId, IsAdmin);
            return Ok(result);
        }
        catch (ShortUrlNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ShortUrlDto>> Create(CreateShortUrlRequest request)
    {
        if (!Uri.TryCreate(
                request.OriginalUrl,
                UriKind.Absolute,
                out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp &&
                uri.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest("OriginalUrl must be a valid absolute http/https URL.");
        }

        try
        {
            var result = await _service.CreateAsync(
                request.OriginalUrl,
                CurrentUserId!);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (DuplicateUrlException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id, CurrentUserId!, IsAdmin);
            return NoContent();
        }
        catch (ShortUrlNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdmin => User.IsInRole("Admin");
}
