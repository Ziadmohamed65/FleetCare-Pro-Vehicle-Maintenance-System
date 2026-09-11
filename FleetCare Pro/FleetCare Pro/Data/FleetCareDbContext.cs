using FleetCare_Pro.Models.Domain;
using FleetCare_Pro.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FleetCare_Pro.Data
{

        public class FleetCareDbContext : IdentityDbContext<ApplicationUser>
        {
            public FleetCareDbContext(
                DbContextOptions<FleetCareDbContext> options)
                : base(options)
            {
            }

            public DbSet<Vehicle> Vehicles { get; set; }

            public DbSet<ServiceCategory> ServiceCategories { get; set; }

            public DbSet<ServiceCenter> ServiceCenters { get; set; }

            public DbSet<ServiceRecord> ServiceRecords { get; set; }

            public DbSet<ServiceLineItem> ServiceLineItems { get; set; }

            public DbSet<AuditLog> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // VERY IMPORTANT:
            // IdentityDbContext needs this.
            base.OnModelCreating(modelBuilder);


            // ============================
            // Vehicle
            // ============================

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.VIN)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.PurchasePrice)
                .HasPrecision(18, 2);

            // Vehicle -> Driver
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Driver)
                .WithMany(u => u.AssignedVehicles)
                .HasForeignKey(v => v.DriverId)
                .OnDelete(DeleteBehavior.SetNull);


            // ============================
            // ServiceRecord
            // ============================

            modelBuilder.Entity<ServiceRecord>()
                .Property(sr => sr.TotalCost)
                .HasPrecision(18, 2);

            // Vehicle -> ServiceRecords
            modelBuilder.Entity<ServiceRecord>()
                .HasOne(sr => sr.Vehicle)
                .WithMany(v => v.ServiceRecords)
                .HasForeignKey(sr => sr.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceCenter -> ServiceRecords
            modelBuilder.Entity<ServiceRecord>()
                .HasOne(sr => sr.ServiceCenter)
                .WithMany(sc => sc.ServiceRecords)
                .HasForeignKey(sr => sr.ServiceCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApplicationUser -> ServiceRecords
            modelBuilder.Entity<ServiceRecord>()
                .HasOne(sr => sr.CreatedByUser)
                .WithMany(u => u.ServiceRecordsCreated)
                .HasForeignKey(sr => sr.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================
            // ServiceLineItem
            // ============================

            modelBuilder.Entity<ServiceLineItem>()
                .Property(li => li.Cost)
                .HasPrecision(18, 2);

            // ServiceRecord -> ServiceLineItems
            modelBuilder.Entity<ServiceLineItem>()
                .HasOne(li => li.ServiceRecord)
                .WithMany(sr => sr.ServiceLineItems)
                .HasForeignKey(li => li.ServiceRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceCategory -> ServiceLineItems
            modelBuilder.Entity<ServiceLineItem>()
                .HasOne(li => li.ServiceCategory)
                .WithMany(sc => sc.ServiceLineItems)
                .HasForeignKey(li => li.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================
            // VendorService
            // ============================
            // Many-to-Many:
            //
            // ServiceCenter <-> ServiceCategory
            //
            // VendorService is the junction table.
            //

         


            // ============================
            // AuditLog
            // ============================

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Timestamp)
                .HasDefaultValueSql("GETDATE()");
        }

    }
}

