using Microsoft.EntityFrameworkCore;
using AburaFoundationSite.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AburaFoundationSite.Data
{
    
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Donation> Donations { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Donation>().ToTable("Donations");
            modelBuilder.Entity<TeamMember>().ToTable("TeamMembers");

            modelBuilder.Entity<Donation>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Amount).IsRequired();
                entity.Property(d => d.PhoneNumber).IsRequired();
                entity.Property(d => d.PaymentMethod).IsRequired();
                entity.Property(d => d.Status).IsRequired();
                entity.Property(d => d.Date).IsRequired();
            });
        }
    }
}
