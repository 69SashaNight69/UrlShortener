using UrlShortener.Api.Services;
using Xunit;

namespace UrlShortener.Tests;

public class AboutServiceTests
{
    [Fact]
    public async Task GetTextAsync_NoContentInDb_ReturnsEmptyString()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AboutService(db);

        var result = await service.GetTextAsync();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task UpdateTextAsync_NoExistingContent_CreatesNewContent()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AboutService(db);

        await service.UpdateTextAsync("New about text.");

        var result = await service.GetTextAsync();
        Assert.Equal("New about text.", result);
    }

    [Fact]
    public async Task UpdateTextAsync_ExistingContent_OverwritesText()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AboutService(db);

        await service.UpdateTextAsync("First version.");
        await service.UpdateTextAsync("Second version.");

        var result = await service.GetTextAsync();
        Assert.Equal("Second version.", result);
    }
}