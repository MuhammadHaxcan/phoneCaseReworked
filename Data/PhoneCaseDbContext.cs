using Microsoft.EntityFrameworkCore;
using phoneCaseReworked.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace phoneCaseReworked.Models {
    public class PhoneCaseDbContext : DbContext {
        public PhoneCaseDbContext(DbContextOptions<PhoneCaseDbContext> options) : base(options) { }

        // DbSets for each model
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PhoneModel> PhoneModels { get; set; }
        public DbSet<CaseManufacturer> CaseManufacturers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            // Purchase → Vendor
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Vendor)
                .WithMany()
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletions.

            // Purchase → Product
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // If a product is deleted, remove purchases.

            // Payment → Vendor
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Vendor)
                .WithMany()
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → PhoneModel
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Model)
                .WithMany()
                .HasForeignKey(p => p.ModelId)
                .OnDelete(DeleteBehavior.SetNull); // Keep product if the phone model is deleted.

            // Product → CaseManufacturer
            modelBuilder.Entity<Product>()
                .HasOne(p => p.CaseManufacturer)
                .WithMany()
                .HasForeignKey(p => p.CaseManufacturerId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PhoneModel>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<CaseManufacturer>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.CaseName, p.CaseManufacturerId, p.ModelId })
                .IsUnique();

        }
    }
}
