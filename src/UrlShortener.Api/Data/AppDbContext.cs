using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();
    public DbSet<AboutContent> AboutContents => Set<AboutContent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ShortUrl>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.OriginalUrl)
                  .IsRequired()
                  .HasMaxLength(2048);

            entity.Property(s => s.ShortCode)
                  .IsRequired(false)
                  .HasMaxLength(10);

            entity.HasIndex(s => s.OriginalUrl).IsUnique();
            entity.HasIndex(s => s.ShortCode).IsUnique();

            entity.HasOne(s => s.CreatedByUser)
                  .WithMany(u => u.ShortUrls)
                  .HasForeignKey(s => s.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AboutContent>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Text).IsRequired();
        });
    }
}