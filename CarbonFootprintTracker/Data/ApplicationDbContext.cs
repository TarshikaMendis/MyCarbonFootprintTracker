using CarbonFootprintTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarbonFootprintTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Carbon Footprint Table
        public DbSet<CarbonRecord> CarbonRecords { get; set; }

        // Carbon Activity Table
        public DbSet<CarbonActivity> CarbonActivities { get; set; }

        //Recommendations Table
        public DbSet<Recommendation> Recommendations { get; set; }
    }
}