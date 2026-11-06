using Microsoft.EntityFrameworkCore;
using TechUI.SampleAdmin.Models;

namespace TechUI.SampleAdmin.Data;

public sealed class AdminDbContext(DbContextOptions<AdminDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(c => c.FullName).HasMaxLength(120).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(160).IsRequired();
            entity.Property(c => c.Phone).HasMaxLength(20);
            entity.Property(c => c.City).HasMaxLength(80).IsRequired();
            entity.Property(c => c.Credit).HasConversion<long>();
            entity.HasIndex(c => c.Email).IsUnique();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(160).IsRequired();
            entity.Property(p => p.Sku).HasMaxLength(40).IsRequired();
            entity.Property(p => p.Price).HasConversion<long>();
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Number).HasMaxLength(30).IsRequired();
            entity.Property(o => o.Total).HasConversion<long>();
            entity.HasIndex(o => o.Number).IsUnique();
            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
