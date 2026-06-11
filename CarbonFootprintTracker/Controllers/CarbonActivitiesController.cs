using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CarbonFootprintTracker.Models;
using CarbonFootprintTracker.Data;
using CarbonFootprintTracker.Services;
using System.Security.Claims;

namespace CarbonFootprintTracker.Controllers
{
    [Authorize]
    public class CarbonActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarbonActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Activities
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var data = await _context.CarbonActivities
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return View(data);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarbonActivity carbonactivity)
        {
            // Get the current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Manual validation
            if (string.IsNullOrEmpty(carbonactivity.ActivityType))
            {
                ModelState.AddModelError("ActivityType", "Activity Type is required");
                return View(carbonactivity);
            }

            if (carbonactivity.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than 0");
                return View(carbonactivity);
            }

            if (string.IsNullOrEmpty(carbonactivity.Unit))
            {
                ModelState.AddModelError("Unit", "Unit is required");
                return View(carbonactivity);
            }

            try
            {
                // Calculate emission
                double emission = CarbonCalculator.Calculate(
                    carbonactivity.ActivityType,
                    carbonactivity.Amount,
                    carbonactivity.Unit);

                // Set values
                carbonactivity.UserId = userId;
                carbonactivity.CarbonEmission = emission;
                carbonactivity.Date = DateTime.Now;

                // Save activity
                _context.CarbonActivities.Add(carbonactivity);
                await _context.SaveChangesAsync();

                // Update or create carbon record
                await UpdateCarbonRecord(userId, carbonactivity.ActivityType, emission);

                // Calculate points earned for this activity
                int pointsEarned = PointsCalculator.CalculatePoints(
                    carbonactivity.ActivityType,
                    carbonactivity.Amount,
                    emission);

                // Check and award rewards based on new total points
                await CheckAndAwardRewards(userId);

                TempData["Success"] = $"Activity added! Carbon emission: {emission} kg CO₂ | +{pointsEarned} points earned!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving activity: {ex.Message}");
                return View(carbonactivity);
            }
        }

        private async Task UpdateCarbonRecord(string userId, string activityType, double emission)
        {
            // Find existing record for this user
            var record = await _context.CarbonRecords
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (record == null)
            {
                // Create new record
                record = new CarbonRecord
                {
                    UserId = userId,
                    TransportEmission = 0,
                    ElectricityEmission = 0,
                    FoodEmission = 0,
                    WasteEmission = 0,
                    TotalEmission = 0,
                    Date = DateTime.Now
                };
                _context.CarbonRecords.Add(record);
            }

            // Update the appropriate category
            switch (activityType)
            {
                case "Transport":
                    record.TransportEmission += emission;
                    break;
                case "Electricity":
                    record.ElectricityEmission += emission;
                    break;
                case "Food":
                    record.FoodEmission += emission;
                    break;
                case "Waste":
                    record.WasteEmission += emission;
                    break;
            }

            // Recalculate total
            record.TotalEmission = record.TransportEmission +
                                   record.ElectricityEmission +
                                   record.FoodEmission +
                                   record.WasteEmission;
            record.Date = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        private async Task CheckAndAwardRewards(string userId)
        {
            // Get user's total points
            var activities = await _context.CarbonActivities
                .Where(a => a.UserId == userId)
                .ToListAsync();

            int totalPoints = PointsCalculator.CalculateTotalUserPoints(activities);

            // Calculate category totals
            double transportTotal = activities.Where(a => a.ActivityType == "Transport").Sum(a => a.CarbonEmission);
            double electricityTotal = activities.Where(a => a.ActivityType == "Electricity").Sum(a => a.CarbonEmission);
            double foodTotal = activities.Where(a => a.ActivityType == "Food").Sum(a => a.CarbonEmission);
            double wasteTotal = activities.Where(a => a.ActivityType == "Waste").Sum(a => a.CarbonEmission);
            int activityCount = activities.Count;

            // Get all available rewards
            var allRewards = await _context.Rewards.Where(r => r.IsActive).ToListAsync();

            // Get already earned rewards
            var earnedRewards = await _context.UserRewards
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RewardId)
                .ToListAsync();

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

                    // Add a special temp message for new rewards
                    TempData["NewReward"] = $"🎉 Congratulations! You earned the '{reward.Name}' badge! 🎉";
                }
            }

            await _context.SaveChangesAsync();
        }

        // Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = await _context.CarbonActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (activity == null)
                return NotFound();

            return View(activity);
        }

        // Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = await _context.CarbonActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (activity == null)
                return NotFound();

            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarbonActivity activity)
        {
            if (id != activity.Id)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Manual validation
            if (string.IsNullOrEmpty(activity.ActivityType))
            {
                ModelState.AddModelError("ActivityType", "Activity Type is required");
                return View(activity);
            }

            if (activity.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than 0");
                return View(activity);
            }

            if (string.IsNullOrEmpty(activity.Unit))
            {
                ModelState.AddModelError("Unit", "Unit is required");
                return View(activity);
            }

            try
            {
                activity.UserId = userId;

                activity.CarbonEmission = CarbonCalculator.Calculate(
                    activity.ActivityType,
                    activity.Amount,
                    activity.Unit);

                _context.Update(activity);
                await _context.SaveChangesAsync();

                // Update carbon record after edit
                await RecalculateUserCarbonRecord(userId);

                // Check and award rewards after edit
                await CheckAndAwardRewards(userId);

                TempData["Success"] = "Activity updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating activity: {ex.Message}");
                return View(activity);
            }
        }

        private async Task RecalculateUserCarbonRecord(string userId)
        {
            // Get all activities for this user
            var activities = await _context.CarbonActivities
                .Where(a => a.UserId == userId)
                .ToListAsync();

            // Calculate totals by category
            double transportTotal = activities.Where(a => a.ActivityType == "Transport").Sum(a => a.CarbonEmission);
            double electricityTotal = activities.Where(a => a.ActivityType == "Electricity").Sum(a => a.CarbonEmission);
            double foodTotal = activities.Where(a => a.ActivityType == "Food").Sum(a => a.CarbonEmission);
            double wasteTotal = activities.Where(a => a.ActivityType == "Waste").Sum(a => a.CarbonEmission);

            var record = await _context.CarbonRecords
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (record != null)
            {
                record.TransportEmission = transportTotal;
                record.ElectricityEmission = electricityTotal;
                record.FoodEmission = foodTotal;
                record.WasteEmission = wasteTotal;
                record.TotalEmission = transportTotal + electricityTotal + foodTotal + wasteTotal;
                record.Date = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        // Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = await _context.CarbonActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (activity == null)
                return NotFound();

            return View(activity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = await _context.CarbonActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (activity != null)
            {
                _context.CarbonActivities.Remove(activity);
                await _context.SaveChangesAsync();

                // Recalculate carbon record after deletion
                await RecalculateUserCarbonRecord(userId);

                // Check and award rewards after deletion
                await CheckAndAwardRewards(userId);

                TempData["Success"] = "Activity deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}