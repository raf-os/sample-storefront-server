using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using SampleStorefront.Models;

namespace SampleStorefront.Context;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserAvatar> UserAvatars { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ImageUpload> ImageUploads { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=sqlite/app.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<User>()
            .HasOne(u => u.Avatar)
            .WithOne(a => a.User)
            .HasForeignKey<UserAvatar>(a => a.UserId)
            .IsRequired();

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(pc => new { pc.ProductId, pc.CategoryId });
        });

        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId);
        
        // Product / image relationships
        modelBuilder.Entity<ProductImage>()
            .HasKey(pi => new { pi.ProductId, pi.ImageUploadId });

        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId);

        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.ImageUpload)
            .WithMany()
            .HasForeignKey(pi => pi.ImageUploadId);

        // Cart / wishlist relationships
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId);

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(ci => new { ci.UserId, ci.ProductId })
                .IsUnique();

            entity.HasIndex(ci => ci.UserId);
        });
    }
}