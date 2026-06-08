
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarbonFootprintTracker.Models;
using CarbonFootprintTracker.Data;
using System.Security.Claims;

namespace CarbonFootprintTracker.Controllers
{
    public class CarbonActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarbonActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CarbonActivities
        public async Task<IActionResult> Index()
        {
            //  GET LOGGED-IN USER ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //  SHOW ONLY USER'S DATA
            var data = await _context.CarbonActivities
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return View(data);
        }

        // GET: Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var carbonactivity = await _context.CarbonActivities
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (carbonactivity == null)
                return NotFound();

            return View(carbonactivity);
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
            if (ModelState.IsValid)
            {
                //  GET LOGGED-IN USER ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                //  ASSIGN USER ID
                carbonactivity.UserId = userId;

                //  CALCULATE EMISSION
                carbonactivity.CarbonEmission =
                    CarbonCalculator.Calculate(
                        carbonactivity.ActivityType,
                        carbonactivity.Amount,
                        carbonactivity.Unit
                    );

                //  SET DATE SAFETY
                if (carbonactivity.Date == default)
                {
                    carbonactivity.Date = DateTime.Now;
                }

                //  SAVE TO DB
                _context.Add(carbonactivity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(carbonactivity);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var carbonactivity = await _context.CarbonActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (carbonactivity == null)
                return NotFound();

            return View(carbonactivity);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarbonActivity carbonactivity)
        {
            if (id != carbonactivity.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    //  ENSURE USER OWNERSHIP
                    carbonactivity.UserId = userId;

                    //  RECALCULATE EMISSION
                    carbonactivity.CarbonEmission =
                        CarbonCalculator.Calculate(
                            carbonactivity.ActivityType,
                            carbonactivity.Amount,
                            carbonactivity.Unit
                        );

                    _context.Update(carbonactivity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarbonActivityExists(carbonactivity.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(carbonactivity);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var carbonactivity = await _context.CarbonActivities
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (carbonactivity == null)
                return NotFound();

            return View(carbonactivity);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var carbonactivity = await _context.CarbonActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (carbonactivity != null)
            {
                _context.CarbonActivities.Remove(carbonactivity);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // CHECK EXISTS (USER SAFE)
        private bool CarbonActivityExists(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return _context.CarbonActivities
                .Any(e => e.Id == id && e.UserId == userId);
        }
    }
}