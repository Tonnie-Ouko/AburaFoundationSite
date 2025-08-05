using Microsoft.EntityFrameworkCore;
using AburaFoundationSite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AburaFoundationSite.Data
{
    // Define AppUser to ensure Identity works well with custom extensions later if needed
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Donation> Donations { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Must call base to let Identity configure its tables correctly
            base.OnModelCreating(modelBuilder);

            // Custom mappings for Donation
            modelBuilder.Entity<Donation>().ToTable("Donations");
            modelBuilder.Entity<Donation>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Amount).IsRequired();
                entity.Property(d => d.PhoneNumber).IsRequired();
                entity.Property(d => d.PaymentMethod).IsRequired();
                entity.Property(d => d.Status).IsRequired();
                entity.Property(d => d.Date).IsRequired();
            });

            // Custom mappings for TeamMember
            modelBuilder.Entity<TeamMember>().ToTable("TeamMembers");
        }
    }
}
