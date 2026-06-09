
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CarbonFootprintTracker.Models;
using CarbonFootprintTracker.Data;
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
            // Remove validation for fields we don't want to validate from form
            ModelState.Remove("CarbonEmission");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                // connect user
                carbonactivity.UserId = userId;

                // calculate emission
                double emission = CarbonCalculator.Calculate(
                    carbonactivity.ActivityType,
                    carbonactivity.Amount,
                    carbonactivity.Unit);

                carbonactivity.CarbonEmission = emission;
                carbonactivity.Date = DateTime.Now;

                // save activity
                _context.CarbonActivities.Add(carbonactivity);
                await _context.SaveChangesAsync();

                // Update or create carbon record
                await UpdateCarbonRecord(userId, carbonactivity.ActivityType, emission);

                TempData["Success"] = $"Activity added! Carbon emission: {emission} kg CO₂";
                return RedirectToAction(nameof(Index));
            }

            return View(carbonactivity);
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

            // Remove validation for fields we set manually
            ModelState.Remove("CarbonEmission");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                activity.UserId = userId;

                activity.CarbonEmission = CarbonCalculator.Calculate(
                    activity.ActivityType,
                    activity.Amount,
                    activity.Unit);

                _context.Update(activity);
                await _context.SaveChangesAsync();

                // Update carbon record after edit
                await RecalculateUserCarbonRecord(userId);

                TempData["Success"] = "Activity updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(activity);
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

                TempData["Success"] = "Activity deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}