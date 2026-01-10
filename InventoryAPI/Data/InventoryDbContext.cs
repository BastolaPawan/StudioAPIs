using InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace InventoryAPI.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

        public DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }
        public DbSet<ItemGroup> ItemGroups { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure decimal precision for Price
            modelBuilder.Entity<Item>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)"); // 18 total digits, 2 after decimal

            // Requirement: Unique Item Code
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.ItemCode)
                .IsUnique();
        }
    }
}