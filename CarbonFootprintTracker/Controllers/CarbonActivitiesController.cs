
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarbonFootprintTracker.Models;
using CarbonFootprintTracker.Data;

public class CarbonActivitiesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CarbonActivitiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: CARBONACTIVITYS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.CarbonActivities.ToListAsync());
    }

    // GET: CARBONACTIVITYS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var carbonactivity = await _context.CarbonActivities
            .FirstOrDefaultAsync(m => m.Id == id);
        if (carbonactivity == null)
        {
            return NotFound();
        }

        return View(carbonactivity);
    }

    // GET: CARBONACTIVITYS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CARBONACTIVITYS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ActivityType,Amount,Unit,CarbonEmission,Date,UserId")] CarbonActivity carbonactivity)
    {
        if (ModelState.IsValid)
        {
            _context.Add(carbonactivity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(carbonactivity);
    }

    // GET: CARBONACTIVITYS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var carbonactivity = await _context.CarbonActivities.FindAsync(id);
        if (carbonactivity == null)
        {
            return NotFound();
        }
        return View(carbonactivity);
    }

    // POST: CARBONACTIVITYS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,ActivityType,Amount,Unit,CarbonEmission,Date,UserId")] CarbonActivity carbonactivity)
    {
        if (id != carbonactivity.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(carbonactivity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarbonActivityExists(carbonactivity.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(carbonactivity);
    }

    // GET: CARBONACTIVITYS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var carbonactivity = await _context.CarbonActivities
            .FirstOrDefaultAsync(m => m.Id == id);
        if (carbonactivity == null)
        {
            return NotFound();
        }

        return View(carbonactivity);
    }

    // POST: CARBONACTIVITYS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var carbonactivity = await _context.CarbonActivities.FindAsync(id);
        if (carbonactivity != null)
        {
            _context.CarbonActivities.Remove(carbonactivity);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CarbonActivityExists(int? id)
    {
        return _context.CarbonActivities.Any(e => e.Id == id);
    }
}
