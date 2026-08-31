using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = services.GetRequiredService<AppDbContext>();

        foreach (var role in new[] { "User", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await EnsureUserAsync(userManager, "user@test.com", "User123!", "User");
        await EnsureUserAsync(userManager, "admin@test.com", "Admin123!", "Admin");

        await EnsureAboutContentAsync(dbContext);
    }

    private static async Task EnsureAboutContentAsync(AppDbContext dbContext)
    {
        if (await dbContext.AboutContents.AnyAsync())
        {
            return;
        }

        dbContext.AboutContents.Add(new AboutContent
        {
            Text = "This URL Shortener uses a Base62 encoding algorithm. " +
                   "Each URL is saved to the database, which generates a unique numeric Id. " +
                   "That Id is then converted into a short Base62 string (using a-z, A-Z, 0-9), " +
                   "which becomes the ShortCode. Since the Id is guaranteed unique by the database, " +
                   "the resulting ShortCode is always unique as well, with no collisions possible.",
            LastUpdated = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureUserAsync(
        UserManager<AppUser> userManager, string email, string password, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return;
        }

        var user = new AppUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed user {email}: {errors}");
        }

        await userManager.AddToRoleAsync(user, role);
    }
}