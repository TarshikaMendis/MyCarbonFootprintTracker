using CarbonFootprintTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace CarbonFootprintTracker.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Check if rewards already exist
            if (context.Rewards.Any())
            {
                return; // Database has already been seeded
            }

            var rewards = new Reward[]
            {
                new Reward
                {
                    Name = "First Step",
                    BadgeIcon = "🌱",
                    RequiredPoints = 10,
                    Category = "General",
                    Description = "Added your first activity",
                    IsActive = true,
                    RequiredEmissionReduction = 0,
                    RequiredActivities = 0
                },
                new Reward
                {
                    Name = "Green Commuter",
                    BadgeIcon = "🚲",
                    RequiredPoints = 0,
                    Category = "Transport",
                    Description = "Keep transport emissions low (<5 kg CO₂)",
                    IsActive = true,
                    RequiredEmissionReduction = 5,
                    RequiredActivities = 0
                },
                new Reward
                {
                    Name = "Energy Saver",
                    BadgeIcon = "💡",
                    RequiredPoints = 0,
                    Category = "Electricity",
                    Description = "Keep electricity emissions low (<10 kg CO₂)",
                    IsActive = true,
                    RequiredEmissionReduction = 10,
                    RequiredActivities = 0
                },
                new Reward
                {
                    Name = "Plant Lover",
                    BadgeIcon = "🥗",
                    RequiredPoints = 0,
                    Category = "Food",
                    Description = "Keep food emissions low (<8 kg CO₂)",
                    IsActive = true,
                    RequiredEmissionReduction = 8,
                    RequiredActivities = 0
                },
                new Reward
                {
                    Name = "Recycling Hero",
                    BadgeIcon = "♻️",
                    RequiredPoints = 0,
                    Category = "Waste",
                    Description = "Keep waste emissions low (<5 kg CO₂)",
                    IsActive = true,
                    RequiredEmissionReduction = 5,
                    RequiredActivities = 0
                },
                new Reward
                {
                    Name = "Carbon Champion",
                    BadgeIcon = "🌟",
                    RequiredPoints = 100,
                    Category = "General",
                    Description = "Earn 100 total points",
                    IsActive = true,
                    RequiredEmissionReduction = 0,
                    RequiredActivities = 0
                },
                new Reward
                {
                    Name = "Eco Warrior",
                    BadgeIcon = "🏆",
                    RequiredPoints = 200,
                    Category = "General",
                    Description = "Earn 200 total points",
                    IsActive = true,
                    RequiredEmissionReduction = 0,
                    RequiredActivities = 0
                }
            };

            context.Rewards.AddRange(rewards);
            context.SaveChanges();
        }

        //  Seed Admin User
        public static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            // Check if admin already exists
            var adminUser = await userManager.FindByEmailAsync("admin@123");

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@123",
                    Email = "admin@123",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now,
                    IsAdmin = true  // Set as admin
                };

                var result = await userManager.CreateAsync(admin, "adminpass");

                if (result.Succeeded)
                {
                    // Admin created successfully
                }
            }
        }
    }
}