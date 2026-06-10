using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarbonFootprintTracker.Data;
using CarbonFootprintTracker.Models;
using System.Security.Claims;

namespace CarbonFootprintTracker.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            // Get current logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Get user's carbon record
            var carbonRecord = await _context.CarbonRecords
                .FirstOrDefaultAsync(r => r.UserId == userId);

            // Get all activities for this user
            var activities = await _context.CarbonActivities
                .Where(a => a.UserId == userId)
                .ToListAsync();

            // Get this month's activities
            var currentDate = DateTime.Now;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

            var thisMonthActivities = activities
                .Where(a => a.Date >= startOfMonth)
                .ToList();

            double thisMonthEmission = thisMonthActivities.Sum(a => a.CarbonEmission);

            // Calculate category totals
            double transportTotal = carbonRecord?.TransportEmission ?? 0;
            double electricityTotal = carbonRecord?.ElectricityEmission ?? 0;
            double foodTotal = carbonRecord?.FoodEmission ?? 0;
            double wasteTotal = carbonRecord?.WasteEmission ?? 0;
            double totalEmission = carbonRecord?.TotalEmission ?? 0;

            // Find highest emission source
            var emissions = new Dictionary<string, double>
            {
                { "Transport", transportTotal },
                { "Electricity", electricityTotal },
                { "Food", foodTotal },
                { "Waste", wasteTotal }
            };

            var highestSource = emissions.OrderByDescending(x => x.Value).FirstOrDefault();
            string highestSourceName = highestSource.Key;
            double highestSourceValue = highestSource.Value;

            // Generate AI Recommendation (Rule-based)
            var recommendation = GenerateRecommendation(
                transportTotal, electricityTotal, foodTotal, wasteTotal, totalEmission, highestSourceName);

            // Create ViewModel
            var viewModel = new DashboardViewModel
            {
                UserName = user?.UserName ?? "User",
                UserEmail = user?.Email ?? "",
                TotalEmission = totalEmission,
                TransportEmission = transportTotal,
                ElectricityEmission = electricityTotal,
                FoodEmission = foodTotal,
                WasteEmission = wasteTotal,
                ThisMonthEmission = thisMonthEmission,
                HighestEmissionSource = highestSourceName,
                HighestEmissionValue = highestSourceValue,
                AiRecommendation = recommendation.Message,
                RecommendationIcon = recommendation.Icon,
                TotalActivities = activities.Count
            };

            return View(viewModel);
        }

        private (string Message, string Icon) GenerateRecommendation(
            double transport, double electricity, double food, double waste, double totalEmission, string highestSource)
        {
            // Rule-based AI Recommendation System

            // Case 1: No data yet
            if (transport == 0 && electricity == 0 && food == 0 && waste == 0)
            {
                return ("Start adding your daily activities to see personalized recommendations!", "📝");
            }

            // Case 2: Transport is highest
            if (highestSource == "Transport" && transport > 10)
            {
                return ("🚗 Try using public transport, cycling, or walking for short distances. Carpooling can also reduce your carbon footprint significantly!", "🚲");
            }

            // Case 3: Electricity is highest
            if (highestSource == "Electricity" && electricity > 10)
            {
                return ("💡 Save electricity by turning off lights when not needed, using LED bulbs, and unplugging devices on standby. Consider energy-efficient appliances!", "🔌");
            }

            // Case 4: Food is highest
            if (highestSource == "Food" && food > 10)
            {
                return ("🥩 Reduce meat consumption and try plant-based meals. Local and seasonal foods have lower carbon footprints!", "🌱");
            }

            // Case 5: Waste is highest
            if (highestSource == "Waste" && waste > 5)
            {
                return ("🗑️ Reduce, reuse, recycle! Compost organic waste and avoid single-use plastics. Every small action helps!", "♻️");
            }

            // Case 6: General recommendations based on specific conditions
            if (transport > 20)
            {
                return ("🚌 Your transport emissions are high. Consider switching to electric vehicles or public transportation!", "🚆");
            }

            if (electricity > 30)
            {
                return ("⚡ High electricity usage detected. Try using natural light and air-drying clothes instead of dryers!", "💡");
            }

            if (food > 15)
            {
                return ("🍽️ Food choices matter! Try reducing food waste and choosing locally sourced produce!", "🥗");
            }

            // Case 7: User is doing well
            if (totalEmission < 20)
            {
                return ("🌟 Great job! Your carbon footprint is low. Keep up the eco-friendly habits and inspire others!", "🏆");
            }

            // Default recommendation
            return ("🌍 Every small step counts! Track your daily activities to get personalized tips for reducing your carbon footprint.", "💚");
        }
    }
}