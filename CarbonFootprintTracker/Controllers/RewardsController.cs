using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarbonFootprintTracker.Data;
using CarbonFootprintTracker.Models;
using CarbonFootprintTracker.Services;
using System.Security.Claims;

namespace CarbonFootprintTracker.Controllers
{
    [Authorize]
    public class RewardsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RewardsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Rewards
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            // Get user's activities
            var activities = await _context.CarbonActivities
                .Where(a => a.UserId == userId)
                .ToListAsync();

            // Calculate total points
            int totalPoints = PointsCalculator.CalculateTotalUserPoints(activities);

            // Get all rewards
            var allRewards = await _context.Rewards
                .Where(r => r.IsActive)
                .ToListAsync();

            // Get user's earned rewards
            var earnedRewards = await _context.UserRewards
                .Include(r => r.Reward)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            var earnedRewardIds = earnedRewards.Select(er => er.RewardId).ToHashSet();

            // Calculate category totals for category-based badges
            double transportTotal = activities.Where(a => a.ActivityType == "Transport").Sum(a => a.CarbonEmission);
            double electricityTotal = activities.Where(a => a.ActivityType == "Electricity").Sum(a => a.CarbonEmission);
            double foodTotal = activities.Where(a => a.ActivityType == "Food").Sum(a => a.CarbonEmission);
            double wasteTotal = activities.Where(a => a.ActivityType == "Waste").Sum(a => a.CarbonEmission);
            double totalEmission = transportTotal + electricityTotal + foodTotal + wasteTotal;

            // Check and award new rewards
            await CheckAndAwardRewards(userId, totalPoints, activities.Count, transportTotal, electricityTotal, foodTotal, wasteTotal, totalEmission);

            // Refresh earned rewards after awarding
            earnedRewards = await _context.UserRewards
                .Include(r => r.Reward)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            // Prepare view model
            var viewModel = new RewardsViewModel
            {
                TotalPoints = totalPoints,
                UserName = user?.UserName ?? "User",
                AvailableRewards = allRewards,
                EarnedRewards = earnedRewards,
                NextMilestone = GetNextMilestone(totalPoints, allRewards, earnedRewardIds),
                ActivitiesCount = activities.Count
            };

            return View(viewModel);
        }

        private async Task CheckAndAwardRewards(string userId, int totalPoints, int activityCount,
            double transportTotal, double electricityTotal, double foodTotal, double wasteTotal, double totalEmission)
        {
            var allRewards = await _context.Rewards.Where(r => r.IsActive).ToListAsync();
            var earnedRewards = await _context.UserRewards.Where(ur => ur.UserId == userId).Select(ur => ur.RewardId).ToListAsync();

            foreach (var reward in allRewards)
            {
                // Skip if already earned
                if (earnedRewards.Contains(reward.Id))
                    continue;

                bool shouldAward = false;

                // Check conditions based on reward category
                switch (reward.Category)
                {
                    case "General":
                        if (reward.RequiredPoints > 0 && totalPoints >= reward.RequiredPoints)
                            shouldAward = true;
                        else if (reward.RequiredActivities > 0 && activityCount >= reward.RequiredActivities)
                            shouldAward = true;
                        break;

                    case "Transport":
                        if (reward.RequiredEmissionReduction > 0 && transportTotal <= reward.RequiredEmissionReduction)
                            shouldAward = true;
                        else if (reward.RequiredActivities > 0 && activityCount >= reward.RequiredActivities)
                            shouldAward = true;
                        break;

                    case "Electricity":
                        if (reward.RequiredEmissionReduction > 0 && electricityTotal <= reward.RequiredEmissionReduction)
                            shouldAward = true;
                        break;

                    case "Food":
                        if (reward.RequiredEmissionReduction > 0 && foodTotal <= reward.RequiredEmissionReduction)
                            shouldAward = true;
                        break;

                    case "Waste":
                        if (reward.RequiredEmissionReduction > 0 && wasteTotal <= reward.RequiredEmissionReduction)
                            shouldAward = true;
                        break;
                }

                if (shouldAward)
                {
                    var userReward = new UserReward
                    {
                        UserId = userId,
                        RewardId = reward.Id,
                        PointsEarned = reward.RequiredPoints > 0 ? reward.RequiredPoints : totalPoints,
                        EarnedAt = DateTime.Now,
                        IsViewed = false
                    };
                    _context.UserRewards.Add(userReward);
                }
            }

            await _context.SaveChangesAsync();
        }

        private string GetNextMilestone(int currentPoints, List<Reward> allRewards, HashSet<int> earnedRewardIds)
        {
            var nextReward = allRewards
                .Where(r => !earnedRewardIds.Contains(r.Id) && r.RequiredPoints > currentPoints)
                .OrderBy(r => r.RequiredPoints)
                .FirstOrDefault();

            if (nextReward != null)
            {
                int pointsNeeded = nextReward.RequiredPoints - currentPoints;
                return $"🎯 {pointsNeeded} more points to earn '{nextReward.Name}' badge!";
            }

            return "🏆 You've earned all badges! Amazing work!";
        }
    }

    // ViewModel for Rewards Index
    public class RewardsViewModel
    {
        public int TotalPoints { get; set; }
        public string UserName { get; set; }
        public List<Reward> AvailableRewards { get; set; }
        public List<UserReward> EarnedRewards { get; set; }
        public string NextMilestone { get; set; }
        public int ActivitiesCount { get; set; }
    }
}