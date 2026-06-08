
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



        // GET: Activities
        public async Task<IActionResult> Index()
        {

            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);


            var data =
                await _context.CarbonActivities
                .Where(x => x.UserId == userId)
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
        public async Task<IActionResult> Create(
            CarbonActivity carbonactivity)
        {


            if (ModelState.IsValid)
            {

                var userId =
                    User.FindFirstValue(
                    ClaimTypes.NameIdentifier);



                // connect user
                carbonactivity.UserId = userId;



                // calculate emission
                double emission =
                    CarbonCalculator.Calculate(
                    carbonactivity.ActivityType,
                    carbonactivity.Amount,
                    carbonactivity.Unit);



                carbonactivity.CarbonEmission =
                    emission;



                carbonactivity.Date =
                    DateTime.Now;



                // save activity
                _context.CarbonActivities.Add(carbonactivity);



                // create carbon record

                CarbonRecord record =
                    new CarbonRecord();


                record.UserId = userId;


                record.Date =
                    DateTime.Now;



                switch (carbonactivity.ActivityType)
                {

                    case "Transport":

                        record.TransportEmission =
                            emission;

                        break;


                    case "Electricity":

                        record.ElectricityEmission =
                            emission;

                        break;


                    case "Food":

                        record.FoodEmission =
                            emission;

                        break;


                    case "Waste":

                        break;

                }



                record.TotalEmission =
                    record.TransportEmission +
                    record.ElectricityEmission +
                    record.FoodEmission +
                    emission;



                _context.CarbonRecords.Add(record);



                await _context.SaveChangesAsync();



                return RedirectToAction(nameof(Index));

            }


            return View(carbonactivity);

        }







        // Details
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
                return NotFound();


            var userId =
                User.FindFirstValue(
                ClaimTypes.NameIdentifier);



            var activity =
                await _context.CarbonActivities
                .FirstOrDefaultAsync(
                x => x.Id == id &&
                x.UserId == userId);



            if (activity == null)
                return NotFound();



            return View(activity);

        }







        // Edit
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
                return NotFound();


            var userId =
                User.FindFirstValue(
                ClaimTypes.NameIdentifier);



            var activity =
                await _context.CarbonActivities
                .FirstOrDefaultAsync(
                x => x.Id == id &&
                x.UserId == userId);



            if (activity == null)
                return NotFound();



            return View(activity);

        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CarbonActivity activity)
        {


            if (id != activity.Id)
                return NotFound();



            if (ModelState.IsValid)
            {

                var userId =
                    User.FindFirstValue(
                    ClaimTypes.NameIdentifier);



                activity.UserId =
                    userId;



                activity.CarbonEmission =
                    CarbonCalculator.Calculate(
                    activity.ActivityType,
                    activity.Amount,
                    activity.Unit);



                _context.Update(activity);


                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));

            }



            return View(activity);

        }







        // Delete

        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
                return NotFound();



            var userId =
                User.FindFirstValue(
                ClaimTypes.NameIdentifier);



            var activity =
                await _context.CarbonActivities
                .FirstOrDefaultAsync(
                x => x.Id == id &&
                x.UserId == userId);



            if (activity == null)
                return NotFound();



            return View(activity);

        }






        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {

            var userId =
                User.FindFirstValue(
                ClaimTypes.NameIdentifier);



            var activity =
                await _context.CarbonActivities
                .FirstOrDefaultAsync(
                x => x.Id == id &&
                x.UserId == userId);



            if (activity != null)
            {

                _context.CarbonActivities.Remove(activity);

                await _context.SaveChangesAsync();

            }



            return RedirectToAction(nameof(Index));

        }

    }
}