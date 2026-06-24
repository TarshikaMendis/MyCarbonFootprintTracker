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
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var activities = await _context.CarbonActivities
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Date)
                .ToListAsync();

            // Get weekly report (last 7 days)
            var weeklyReport = GetWeeklyReport(activities);

            // Get monthly report (last 30 days)
            var monthlyReport = GetMonthlyReport(activities);

            // Calculate trend (compare current month with previous month)
            var trendData = CalculateTrend(activities);

            var viewModel = new ReportsViewModel
            {
                UserName = user?.UserName ?? "User",
                UserEmail = user?.Email ?? "",
                WeeklyReport = weeklyReport,
                MonthlyReport = monthlyReport,
                EmissionChangePercent = trendData.PercentChange,
                TrendMessage = trendData.Message,
                TrendIcon = trendData.Icon
            };

            return View(viewModel);
        }

        private WeeklyReport GetWeeklyReport(List<CarbonActivity> activities)
        {
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-6);

            var weeklyActivities = activities
                .Where(a => a.Date.Date >= startDate && a.Date.Date <= endDate)
                .ToList();

            var dailyEmissions = new List<DailyEmission>();
            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var dailyTotal = weeklyActivities
                    .Where(a => a.Date.Date == date)
                    .Sum(a => a.CarbonEmission);

                dailyEmissions.Add(new DailyEmission
                {
                    Date = date,
                    DayName = date.ToString("ddd"),
                    Emission = Math.Round(dailyTotal, 2)
                });
            }

            // Calculate category totals for weekly report
            double transportTotal = weeklyActivities.Where(a => a.ActivityType == "Transport").Sum(a => a.CarbonEmission);
            double electricityTotal = weeklyActivities.Where(a => a.ActivityType == "Electricity").Sum(a => a.CarbonEmission);
            double foodTotal = weeklyActivities.Where(a => a.ActivityType == "Food").Sum(a => a.CarbonEmission);
            double wasteTotal = weeklyActivities.Where(a => a.ActivityType == "Waste").Sum(a => a.CarbonEmission);

            var emissions = new Dictionary<string, double>
            {
                { "Transport", transportTotal },
                { "Electricity", electricityTotal },
                { "Food", foodTotal },
                { "Waste", wasteTotal }
            };

            var highest = emissions.OrderByDescending(x => x.Value).FirstOrDefault();

            double totalEmission = weeklyActivities.Sum(a => a.CarbonEmission);

            return new WeeklyReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalEmission = Math.Round(totalEmission, 2),
                TotalActivities = weeklyActivities.Count,
                AverageDailyEmission = Math.Round(weeklyActivities.Any() ? totalEmission / 7 : 0, 2),
                HighestCategory = highest.Key,
                HighestCategoryValue = Math.Round(highest.Value, 2),
                DailyEmissions = dailyEmissions
            };
        }

        private MonthlyReport GetMonthlyReport(List<CarbonActivity> activities)
        {
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-29);

            var monthlyActivities = activities
                .Where(a => a.Date.Date >= startDate && a.Date.Date <= endDate)
                .ToList();

            var weeklyEmissions = new List<WeeklyEmission>();
            for (int i = 0; i < 4; i++)
            {
                var weekStart = startDate.AddDays(i * 7);
                var weekEnd = weekStart.AddDays(6);

                var weeklyTotal = monthlyActivities
                    .Where(a => a.Date.Date >= weekStart && a.Date.Date <= weekEnd)
                    .Sum(a => a.CarbonEmission);

                weeklyEmissions.Add(new WeeklyEmission
                {
                    WeekNumber = i + 1,
                    StartDate = weekStart,
                    EndDate = weekEnd,
                    Emission = Math.Round(weeklyTotal, 2)
                });
            }

            // Calculate category totals for monthly report
            double transportTotal = monthlyActivities.Where(a => a.ActivityType == "Transport").Sum(a => a.CarbonEmission);
            double electricityTotal = monthlyActivities.Where(a => a.ActivityType == "Electricity").Sum(a => a.CarbonEmission);
            double foodTotal = monthlyActivities.Where(a => a.ActivityType == "Food").Sum(a => a.CarbonEmission);
            double wasteTotal = monthlyActivities.Where(a => a.ActivityType == "Waste").Sum(a => a.CarbonEmission);

            var emissions = new Dictionary<string, double>
            {
                { "Transport", transportTotal },
                { "Electricity", electricityTotal },
                { "Food", foodTotal },
                { "Waste", wasteTotal }
            };

            var highest = emissions.OrderByDescending(x => x.Value).FirstOrDefault();

            double totalEmission = monthlyActivities.Sum(a => a.CarbonEmission);

            return new MonthlyReport
            {
                Year = DateTime.Now.Year,
                MonthName = DateTime.Now.ToString("MMMM"),
                TotalEmission = Math.Round(totalEmission, 2),
                TotalActivities = monthlyActivities.Count,
                AverageDailyEmission = Math.Round(monthlyActivities.Any() ? totalEmission / 30 : 0, 2),
                HighestCategory = highest.Key,
                HighestCategoryValue = Math.Round(highest.Value, 2),
                WeeklyEmissions = weeklyEmissions
            };
        }

        private (double PercentChange, string Message, string Icon) CalculateTrend(List<CarbonActivity> activities)
        {
            var currentMonth = DateTime.Now;
            var previousMonth = currentMonth.AddMonths(-1);

            var currentMonthStart = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            var previousMonthStart = new DateTime(previousMonth.Year, previousMonth.Month, 1);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);

            double currentMonthEmission = activities
                .Where(a => a.Date >= currentMonthStart && a.Date <= currentMonthStart.AddMonths(1).AddDays(-1))
                .Sum(a => a.CarbonEmission);

            double previousMonthEmission = activities
                .Where(a => a.Date >= previousMonthStart && a.Date <= previousMonthEnd)
                .Sum(a => a.CarbonEmission);

            if (previousMonthEmission == 0)
            {
                if (currentMonthEmission == 0)
                {
                    return (0, "Start adding activities to see your progress!", "📝");
                }
                return (100, "Great start! Keep tracking your emissions!", "🌟");
            }

            double percentChange = ((currentMonthEmission - previousMonthEmission) / previousMonthEmission) * 100;

            if (percentChange < 0)
            {
                return (Math.Round(Math.Abs(percentChange), 1),
                    $"🎉 Excellent! Your emissions decreased by {Math.Round(Math.Abs(percentChange), 1)}% compared to last month!",
                    "📉");
            }
            else if (percentChange > 0)
            {
                return (Math.Round(percentChange, 1),
                    $"⚠️ Your emissions increased by {Math.Round(percentChange, 1)}% compared to last month. Try following your AI recommendations!",
                    "📈");
            }
            else
            {
                return (0,
                    "✅ Your emissions are stable compared to last month. Keep up the good work!",
                    "✅");
            }
        }
    }
}