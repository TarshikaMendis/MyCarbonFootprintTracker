using System.Security.Claims;
using CarbonFootprintTracker.Data;
using CarbonFootprintTracker.Models;
using CarbonFootprintTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarbonFootprintTracker.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Get current logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            // Check if user is admin
            if (user == null || !user.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            // Get all users
            var users = await _context.Users.ToListAsync();

            // Get all activities with user navigation property
            var allActivities = await _context.CarbonActivities
                .Include(a => a.User)
                .ToListAsync();

            // Get all user rewards
            var userRewards = await _context.UserRewards.ToListAsync();
            var rewards = await _context.Rewards.ToListAsync();

            // Current month data
            var currentDate = DateTime.Now;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

            var activitiesThisMonth = allActivities
                .Where(a => a.Date >= startOfMonth)
                .ToList();

            var usersThisMonth = users
                .Where(u => u.CreatedAt >= startOfMonth)
                .ToList();

            // Calculate emissions by category
            double transportTotal = allActivities.Where(a => a.ActivityType == "Transport").Sum(a => a.CarbonEmission);
            double electricityTotal = allActivities.Where(a => a.ActivityType == "Electricity").Sum(a => a.CarbonEmission);
            double foodTotal = allActivities.Where(a => a.ActivityType == "Food").Sum(a => a.CarbonEmission);
            double wasteTotal = allActivities.Where(a => a.ActivityType == "Waste").Sum(a => a.CarbonEmission);

            // Get recent activities (last 10)
            var recentActivities = allActivities
                .OrderByDescending(a => a.Date)
                .Take(10)
                .Select(a => new RecentActivity
                {
                    UserName = a.User != null ? a.User.UserName ?? "Unknown" : "Unknown",
                    ActivityType = a.ActivityType,
                    Amount = a.Amount,
                    Unit = a.Unit,
                    CarbonEmission = a.CarbonEmission,
                    Date = a.Date
                })
                .ToList();

            // Get top users (by activity count)
            var topUsersData = allActivities
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    TotalEmission = g.Sum(a => a.CarbonEmission)
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var topUserList = new List<TopUser>();
            foreach (var item in topUsersData)
            {
                var userItem = users.FirstOrDefault(u => u.Id == item.UserId);
                if (userItem != null)
                {
                    // Calculate points for this user
                    var userActivities = allActivities.Where(a => a.UserId == item.UserId).ToList();
                    int points = PointsCalculator.CalculateTotalUserPoints(userActivities);

                    topUserList.Add(new TopUser
                    {
                        UserName = userItem.UserName ?? "Unknown",
                        Email = userItem.Email ?? "",
                        ActivityCount = item.Count,
                        TotalEmission = Math.Round(item.TotalEmission, 2),
                        Points = points
                    });
                }
            }

            // Create ViewModel
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = users.Count,
                NewUsersThisMonth = usersThisMonth.Count,
                TotalActivities = allActivities.Count,
                ActivitiesThisMonth = activitiesThisMonth.Count,
                TotalEmissions = Math.Round(allActivities.Sum(a => a.CarbonEmission), 2),
                EmissionsThisMonth = Math.Round(activitiesThisMonth.Sum(a => a.CarbonEmission), 2),
                TotalBadgesEarned = userRewards.Count,
                TotalRewards = rewards.Count,
                TransportEmission = Math.Round(transportTotal, 2),
                ElectricityEmission = Math.Round(electricityTotal, 2),
                FoodEmission = Math.Round(foodTotal, 2),
                WasteEmission = Math.Round(wasteTotal, 2),
                RecentActivities = recentActivities,
                TopUsers = topUserList
            };

            return View(viewModel);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            // Get current logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            // Check if user is admin
            if (user == null || !user.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            var users = await _context.Users.ToListAsync();
            var allActivities = await _context.CarbonActivities.ToListAsync();
            var userRewards = await _context.UserRewards.ToListAsync();

            var userList = new List<UserInfo>();
            foreach (var userItem in users)
            {
                var userActivities = allActivities.Where(a => a.UserId == userItem.Id).ToList();
                var userBadges = userRewards.Where(r => r.UserId == userItem.Id).ToList();
                int points = PointsCalculator.CalculateTotalUserPoints(userActivities);

                userList.Add(new UserInfo
                {
                    Id = userItem.Id,
                    UserName = userItem.UserName ?? "Unknown",
                    Email = userItem.Email ?? "",
                    ActivityCount = userActivities.Count,
                    TotalEmission = Math.Round(userActivities.Sum(a => a.CarbonEmission), 2),
                    Points = points,
                    BadgesEarned = userBadges.Count,
                    RegisteredDate = userItem.CreatedAt
                });
            }

            var viewModel = new AdminUsersViewModel
            {
                Users = userList.OrderByDescending(u => u.ActivityCount).ToList(),
                TotalUsers = users.Count
            };

            return View(viewModel);
        }

        // GET: Admin/Activities
        public async Task<IActionResult> Activities()
        {
            // Get current logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            // Check if user is admin
            if (user == null || !user.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            var activities = await _context.CarbonActivities
                .Include(a => a.User)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            var viewModel = new AdminActivitiesViewModel
            {
                Activities = activities.Select(a => new AdminActivityInfo
                {
                    Id = a.Id,
                    UserName = a.User != null ? a.User.UserName ?? "Unknown" : "Unknown",
                    ActivityType = a.ActivityType,
                    Amount = a.Amount,
                    Unit = a.Unit,
                    CarbonEmission = a.CarbonEmission,
                    Date = a.Date
                }).ToList(),
                TotalActivities = activities.Count,
                TotalEmissions = Math.Round(activities.Sum(a => a.CarbonEmission), 2)
            };

            return View(viewModel);
        }
    }
}