using FleetCare_Pro.Data;
using FleetCare_Pro.Models.Domain;
using FleetCare_Pro.Models.ViewModels.ServiceCenter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetCare_Pro.Controllers
{
        public class ServiceCenterController : Controller
        {
            private readonly FleetCareDbContext _context;

            public ServiceCenterController(FleetCareDbContext context)
            {
                _context = context;
            }



            public async Task<IActionResult> Index()
            {
                var serviceCenters = await _context.ServiceCenters
                    .Include(sc => sc.ServiceRecords)
                    .OrderBy(sc => sc.Name)
                    .ToListAsync();

                return View(serviceCenters);
            }

            public async Task<IActionResult> Details(int id)
            {
                var serviceCenter = await _context.ServiceCenters
                    .Include(sc => sc.ServiceRecords)
                        .ThenInclude(sr => sr.Vehicle)
                    .FirstOrDefaultAsync(sc => sc.Id == id);

                if (serviceCenter == null)
                {
                    return NotFound();
                }

                return View(serviceCenter);
            }



            [HttpGet]
            public IActionResult Create()
            {
                var model = new ServiceCenterCreateViewModel
                {
                    IsActive = true
                };

                return View(model);
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(
                ServiceCenterCreateViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var serviceCenter = new ServiceCenter
                {
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Address = model.Address,
                    IsActive = model.IsActive
                };

                _context.ServiceCenters.Add(serviceCenter);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Service center created successfully.";

                return RedirectToAction(nameof(Index));
            }


            [HttpGet]
            public async Task<IActionResult> Edit(int id)
            {
                var serviceCenter = await _context.ServiceCenters
                    .FirstOrDefaultAsync(sc => sc.Id == id);

                if (serviceCenter == null)
                {
                    return NotFound();
                }

                var model = new ServiceCenterEditViewModel
                {
                    Id = serviceCenter.Id,
                    Name = serviceCenter.Name,
                    PhoneNumber = serviceCenter.PhoneNumber,
                    Email = serviceCenter.Email,
                    Address = serviceCenter.Address,
                    IsActive = serviceCenter.IsActive
                };

                return View(model);
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(
                int id,
                ServiceCenterEditViewModel model)
            {
                if (id != model.Id)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var serviceCenter = await _context.ServiceCenters
                    .FirstOrDefaultAsync(sc => sc.Id == id);

                if (serviceCenter == null)
                {
                    return NotFound();
                }

                serviceCenter.Name = model.Name;
                serviceCenter.PhoneNumber = model.PhoneNumber;
                serviceCenter.Email = model.Email;
                serviceCenter.Address = model.Address;
                serviceCenter.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Service center updated successfully.";

                return RedirectToAction(nameof(Index));
            }


            [HttpGet]
            public async Task<IActionResult> Delete(int id)
            {
                var serviceCenter = await _context.ServiceCenters
                    .FirstOrDefaultAsync(sc => sc.Id == id);

                if (serviceCenter == null)
                {
                    return NotFound();
                }

                return View(serviceCenter);
            }

            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var serviceCenter = await _context.ServiceCenters
                    .FirstOrDefaultAsync(sc => sc.Id == id);

                if (serviceCenter == null)
                {
                    return NotFound();
                }

                // Do not allow deleting a service center
                // that already has service records.
                bool hasServiceRecords = await _context.ServiceRecords
                    .AnyAsync(sr => sr.ServiceCenterId == id);

                if (hasServiceRecords)
                {
                    TempData["Error"] =
                        "This service center cannot be deleted because it has service records.";

                    return RedirectToAction(nameof(Index));
                }

                _context.ServiceCenters.Remove(serviceCenter);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Service center deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
        }
}

