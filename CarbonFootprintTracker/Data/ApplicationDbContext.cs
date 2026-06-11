using CarbonFootprintTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarbonFootprintTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Carbon Record Table
        public DbSet<CarbonRecord> CarbonRecords { get; set; }
        // Carbon Activity Table
        public DbSet<CarbonActivity> CarbonActivities { get; set; }
        // Recommendations Table
        public DbSet<Recommendation> Recommendations { get; set; }
        // Rewards Table
        public DbSet<Reward> Rewards { get; set; }

        // User Rewards Table
        public DbSet<UserReward> UserRewards { get; set; }
    }
}