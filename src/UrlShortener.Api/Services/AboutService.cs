using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;

namespace UrlShortener.Api.Services;

public class AboutService
{
    private readonly AppDbContext _dbContext;

    public AboutService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GetTextAsync()
    {
        var content = await _dbContext.AboutContents.FirstOrDefaultAsync();
        return content?.Text ?? string.Empty;
    }

    public async Task UpdateTextAsync(string newText)
    {
        var content = await _dbContext.AboutContents.FirstOrDefaultAsync();
        if (content is null)
        {
            content = new Models.AboutContent
            {
                Text = newText,
                LastUpdated = DateTime.UtcNow
            };
            _dbContext.AboutContents.Add(content);
        }
        else
        {
            content.Text = newText;
            content.LastUpdated = DateTime.UtcNow;
        }
        await _dbContext.SaveChangesAsync();
    }
}
