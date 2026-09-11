using FleetCare_Pro.Data;
using FleetCare_Pro.Models.Domain;
using FleetCare_Pro.Models.ViewModels.ServiceCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetCare_Pro.Controllers
{
    public class ServiceCategoryController : Controller
    {
        private readonly FleetCareDbContext _context;

        public ServiceCategoryController(FleetCareDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var categories = await _context.ServiceCategories
                .Include(sc => sc.ServiceLineItems)
                .OrderBy(sc => sc.CategoryName)
                .ToListAsync();

            return View(categories);
        }


        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.ServiceCategories
                .Include(sc => sc.ServiceLineItems)
                    .ThenInclude(li => li.ServiceRecord)
                        .ThenInclude(sr => sr.Vehicle)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ServiceCategoryCreateViewModel());
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ServiceCategoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Prevent duplicate category names
            bool categoryExists = await _context.ServiceCategories
                .AnyAsync(sc =>
                    sc.CategoryName.ToLower() ==
                    model.CategoryName.ToLower());

            if (categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryName),
                    "A service category with this name already exists.");

                return View(model);
            }

            var category = new ServiceCategory
            {
                CategoryName = model.CategoryName.Trim(),
                Description = model.Description?.Trim(),
                RecommendedIntervalMonths =
                    model.RecommendedIntervalMonths
            };

            _context.ServiceCategories.Add(category);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Service category created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.ServiceCategories
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var model = new ServiceCategoryEditViewModel
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                RecommendedIntervalMonths =
                    category.RecommendedIntervalMonths
            };

            return View(model);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ServiceCategoryEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _context.ServiceCategories
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Prevent duplicate category names
            bool categoryExists = await _context.ServiceCategories
                .AnyAsync(sc =>
                    sc.Id != id &&
                    sc.CategoryName.ToLower() ==
                    model.CategoryName.ToLower());

            if (categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryName),
                    "A service category with this name already exists.");

                return View(model);
            }

            category.CategoryName = model.CategoryName.Trim();
            category.Description = model.Description?.Trim();
            category.RecommendedIntervalMonths =
                model.RecommendedIntervalMonths;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Service category updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.ServiceCategories
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.ServiceCategories
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Check if category is used by service line items
            bool hasServiceLineItems =
                await _context.ServiceLineItems
                    .AnyAsync(li => li.ServiceCategoryId == id);

            if (hasServiceLineItems)
            {
                TempData["Error"] =
                    "This service category cannot be deleted because it is used by service records.";

                return RedirectToAction(nameof(Index));
            }

            _context.ServiceCategories.Remove(category);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Service category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}

