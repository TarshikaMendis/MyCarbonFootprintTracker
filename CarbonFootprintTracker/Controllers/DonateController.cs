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
    public class DonateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ImpactService _impactService;

        public DonateController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ImpactService impactService)
        {
            _context = context;
            _userManager = userManager;
            _impactService = impactService;
        }

        // GET: Donate/Index
        public IActionResult Index()
        {
            var impact = _impactService.GetImpactStats();
            ViewBag.Impact = impact;
            return View();
        }

        // POST: Donate/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Donation donation)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get current user ID if logged in
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        donation.UserId = userId;
                    }

                    // Generate transaction ID
                    donation.TransactionId = $"LEAF-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

                    // Mask card details for security
                    if (!string.IsNullOrEmpty(donation.CardNumber))
                    {
                        donation.CardNumber = "****" + donation.CardNumber.Substring(Math.Max(0, donation.CardNumber.Length - 4));
                    }
                    if (!string.IsNullOrEmpty(donation.CardCVV))
                    {
                        donation.CardCVV = "***";
                    }

                    donation.Status = "Completed";
                    donation.DonationDate = DateTime.Now;

                    _context.Donations.Add(donation);
                    await _context.SaveChangesAsync();

                    // Update impact stats
                    _impactService.UpdateImpact(donation.Amount);

                    // Redirect to success page
                    return RedirectToAction("Success", new { transactionId = donation.TransactionId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error processing donation: {ex.Message}");
                }
            }

            var impact = _impactService.GetImpactStats();
            ViewBag.Impact = impact;
            return View(donation);
        }

        // GET: Donate/Success
        public IActionResult Success(string transactionId)
        {
            ViewBag.TransactionId = transactionId;
            var impact = _impactService.GetImpactStats();
            ViewBag.Impact = impact;
            return View();
        }

        // GET: Donate/AdminDonations (Admin Only)
        [Authorize]
        public async Task<IActionResult> AdminDonations()
        {
            // Check if user is admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            var donations = await _context.Donations
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            var totalAmount = donations.Sum(d => d.Amount);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.TotalDonations = donations.Count;

            return View(donations);
        }
    }
}