using AburaFoundationSite.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AburaFoundationSite.Models;

namespace AburaFoundationSite.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Donation> Donations { get; set; }
    }
}
