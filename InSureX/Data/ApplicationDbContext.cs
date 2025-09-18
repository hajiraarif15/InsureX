using InSureX.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace InSureX.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Claim>()
                .HasOne(c => c.Employee)
                .WithMany() 
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Claim)
                .WithMany()
                .HasForeignKey(p => p.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Claim>()
        .Property(c => c.Amount)
        .HasPrecision(18, 2);

            builder.Entity<Claim>()
                .Property(c => c.ApprovedAmount)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.AmountPaid)
                .HasPrecision(18, 2);

        }
    }
}
