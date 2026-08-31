using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.DTOs;
using UrlShortener.Api.Exceptions;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public class ShortUrlService
{
    private readonly AppDbContext _dbContext;

    public ShortUrlService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ShortUrlDto>> GetAllAsync(string? currentId, bool isAdmin)
    {
        var items = await _dbContext.ShortUrls
            .Include(s => s.CreatedByUser)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();

        return items.Select(s => ToDto(s, currentId, isAdmin)).ToList();
    }

    public async Task<ShortUrlDto?> GetByIdAsync(int id, string? currentId, bool isAdmin)
    {
        var item = await _dbContext.ShortUrls
            .Include(s => s.CreatedByUser)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new ShortUrlNotFoundException();

        return ToDto(item, currentId, isAdmin);
    }

    public async Task<ShortUrlDto> CreateAsync(string originalUrl, string userId)
    {
        var exists = await _dbContext.ShortUrls.AnyAsync(s => s.OriginalUrl == originalUrl);
        if (exists)
        {
            throw new DuplicateUrlException(originalUrl);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var entity = new ShortUrl
            {
                OriginalUrl = originalUrl,
                CreatedDate = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            _dbContext.ShortUrls.Add(entity);
            await _dbContext.SaveChangesAsync();

            entity.ShortCode = Base62Encoder.Encode(entity.Id);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            await _dbContext.Entry(entity).Reference(s => s.CreatedByUser).LoadAsync();

            return ToDto(entity, userId, isAdmin: false);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            var duplicateExists = await _dbContext.ShortUrls
                .AsNoTracking()
                .AnyAsync(s => s.OriginalUrl == originalUrl);

            if (duplicateExists)
            {
                throw new DuplicateUrlException(originalUrl);
            }

            throw;
        }
    }


    public async Task DeleteAsync(int id, string currentUserId, bool isAdmin)
    {
        var item = await _dbContext.ShortUrls.FindAsync(id)
            ?? throw new ShortUrlNotFoundException();

        if (!isAdmin && item.CreatedByUserId != currentUserId)
        {
            throw new ForbiddenOperationException("You can only delete your own URLs.");
        }

        _dbContext.ShortUrls.Remove(item);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<string?> GetOriginalUrlAsync(string shortCode)
    {
        var item = await _dbContext.ShortUrls
            .FirstOrDefaultAsync(s => s.ShortCode == shortCode);
        return item?.OriginalUrl;
    }

    private static ShortUrlDto ToDto(ShortUrl s, string? currentUserId, bool isAdmin)
    {
        var canDelete = currentUserId is not null &&
                        (isAdmin || s.CreatedByUserId == currentUserId);

        return new ShortUrlDto(
            s.Id,
            s.OriginalUrl,
            s.ShortCode ?? throw new InvalidOperationException("ShortCode must be generated before mapping to DTO."),
            s.CreatedByUser?.UserName ?? "unknown",
            s.CreatedDate,
            canDelete);
    }
}
