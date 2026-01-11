namespace PaymentIntegrationAPI.Data
{
    using Microsoft.EntityFrameworkCore;
    using PaymentIntegrationAPI.Models;

    public class PaymentIntegrationDbContext : DbContext
    {
        public PaymentIntegrationDbContext(DbContextOptions<PaymentIntegrationDbContext> options) : base(options) { }

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Bill> Bills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PaymentTransaction Configuration
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransactionUuid).IsUnique();
                entity.HasIndex(e => e.BillId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.InitiatedAt);

                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.ServiceCharge).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DeliveryCharge).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)");

                entity.HasOne(e => e.Bill)
                    .WithOne(b => b.PaymentTransaction)
                    .HasForeignKey<PaymentTransaction>(e => e.BillId);
            });

            // Bill Configuration
            modelBuilder.Entity<Bill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BillNumber).IsUnique();
                entity.HasIndex(e => e.PaymentStatus);

                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(10,2)");
            });
        }
    }
}