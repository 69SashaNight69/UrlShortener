using UrlShortener.Api.Data;
using UrlShortener.Api.Exceptions;
using UrlShortener.Api.Models;
using UrlShortener.Api.Services;
using Xunit;

namespace UrlShortener.Tests;

public class ShortUrlServiceTests
{
    private const string UserAId = "user-a-id";
    private const string UserBId = "user-b-id";

    [Fact]
    public async Task CreateAsync_DuplicateOriginalUrl_ThrowsDuplicateUrlException()
    {
        await using var db = TestDbContextFactory.Create();

        await SeedUserAsync(db, UserAId, "userA@test.com");

        var service = new ShortUrlService(db);

        await service.CreateAsync(
            "https://example.com",
            UserAId);

        await Assert.ThrowsAsync<DuplicateUrlException>(
            () => service.CreateAsync(
                "https://example.com",
                UserAId));
    }

    [Fact]
    public async Task DeleteAsync_OwnUrl_DeletesSuccessfully()
    {
        await using var db = TestDbContextFactory.Create();

        await SeedUserAsync(db, UserAId, "userA@test.com");

        var service = new ShortUrlService(db);

        var created = await service.CreateAsync(
            "https://example.com",
            UserAId);

        await service.DeleteAsync(
            created.Id,
            UserAId,
            false);

        var stillExists = db.ShortUrls.Any(
            s => s.Id == created.Id);

        Assert.False(stillExists);
    }

    [Fact]
    public async Task DeleteAsync_OtherUsersUrl_AsRegularUser_ThrowsForbiddenOperationException()
    {
        await using var db = TestDbContextFactory.Create();

        await SeedUserAsync(db, UserAId, "userA@test.com");
        await SeedUserAsync(db, UserBId, "userB@test.com");

        var service = new ShortUrlService(db);

        var created = await service.CreateAsync(
            "https://example.com",
            UserAId);

        await Assert.ThrowsAsync<ForbiddenOperationException>(
            () => service.DeleteAsync(
                created.Id,
                UserBId,
                false));
    }

    [Fact]
    public async Task DeleteAsync_OtherUsersUrl_AsAdmin_DeletesSuccessfully()
    {
        await using var db = TestDbContextFactory.Create();

        await SeedUserAsync(db, UserAId, "userA@test.com");
        await SeedUserAsync(db, UserBId, "adminB@test.com");

        var service = new ShortUrlService(db);

        var created = await service.CreateAsync(
            "https://example.com",
            UserAId);

        await service.DeleteAsync(
            created.Id,
            UserBId,
            true);

        var stillExists = db.ShortUrls.Any(
            s => s.Id == created.Id);

        Assert.False(stillExists);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ThrowsShortUrlNotFoundException()
    {
        await using var db = TestDbContextFactory.Create();

        var service = new ShortUrlService(db);

        await Assert.ThrowsAsync<ShortUrlNotFoundException>(
            () => service.GetByIdAsync(
                999,
                null,
                false));
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_ThrowsShortUrlNotFoundException()
    {
        await using var db = TestDbContextFactory.Create();

        var service = new ShortUrlService(db);

        await Assert.ThrowsAsync<ShortUrlNotFoundException>(
            () => service.DeleteAsync(
                999,
                UserAId,
                false));
    }

    [Fact]
    public async Task GetOriginalUrlAsync_NonExistentShortCode_ReturnsNull()
    {
        await using var db = TestDbContextFactory.Create();

        var service = new ShortUrlService(db);

        var result = await service.GetOriginalUrlAsync(
            "doesnotexist");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ExistingShortCode_ReturnsOriginalUrl()
    {
        await using var db = TestDbContextFactory.Create();

        await SeedUserAsync(db, UserAId, "userA@test.com");

        var service = new ShortUrlService(db);

        var created = await service.CreateAsync(
            "https://example.com",
            UserAId);

        var result = await service.GetOriginalUrlAsync(
            created.ShortCode);

        Assert.Equal(
            "https://example.com",
            result);
    }

    [Fact]
    public async Task GetAllAsync_AnonymousUser_CanDeleteIsAlwaysFalse()
    {
        await using var db = TestDbContextFactory.Create();

        await SeedUserAsync(db, UserAId, "userA@test.com");

        var service = new ShortUrlService(db);

        await service.CreateAsync(
            "https://example.com",
            UserAId);

        var result = await service.GetAllAsync(
            null,
            false);

        Assert.All(
            result,
            dto => Assert.False(dto.CanDelete));
    }

    [Fact]
    public async Task GetAllAsync_OwnerUser_CanDeleteIsTrueForOwnUrl()
    {
        await using var db = TestDbContextFactory.Create();

        await SeedUserAsync(db, UserAId, "userA@test.com");

        var service = new ShortUrlService(db);

        await service.CreateAsync(
            "https://example.com",
            UserAId);

        var result = await service.GetAllAsync(
            UserAId,
            false);

        Assert.True(result.Single().CanDelete);
    }

    [Fact]
    public async Task GetAllAsync_OtherRegularUser_CanDeleteIsFalseForNotOwnedUrl()
    {
        await using var db = TestDbContextFactory.Create();

        await SeedUserAsync(db, UserAId, "userA@test.com");
        await SeedUserAsync(db, UserBId, "userB@test.com");

        var service = new ShortUrlService(db);

        await service.CreateAsync(
            "https://example.com",
            UserAId);

        var result = await service.GetAllAsync(
            UserBId,
            false);

        Assert.False(result.Single().CanDelete);
    }

    [Fact]
    public async Task GetAllAsync_Admin_CanDeleteIsTrueEvenForNotOwnedUrl()
    {
        await using var db = TestDbContextFactory.Create();

        await SeedUserAsync(db, UserAId, "userA@test.com");
        await SeedUserAsync(db, UserBId, "adminB@test.com");

        var service = new ShortUrlService(db);

        await service.CreateAsync(
            "https://example.com",
            UserAId);

        var result = await service.GetAllAsync(
            UserBId,
            true);

        Assert.True(result.Single().CanDelete);
    }

    private static async Task SeedUserAsync(
        AppDbContext db,
        string id,
        string userName)
    {
        db.Users.Add(new AppUser
        {
            Id = id,
            UserName = userName,
            Email = userName
        });

        await db.SaveChangesAsync();
    }
}