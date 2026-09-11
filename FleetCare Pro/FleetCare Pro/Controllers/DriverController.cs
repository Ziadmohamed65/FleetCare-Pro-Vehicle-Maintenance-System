using FleetCare_Pro.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetCare_Pro.Controllers
{
        [Authorize(Roles = "Driver")]
        public class DriverController : Controller
        {
            private readonly FleetCareDbContext _context;

            public DriverController(FleetCareDbContext context)
            {
                _context = context;
            }

            // GET: /Driver/Index
            public async Task<IActionResult> Index()
            {
                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var vehicles = await _context.Vehicles
                    .Where(v => v.DriverId == userId)
                    .Include(v => v.ServiceRecords)
                    .ToListAsync();

                return View(vehicles);
            }

            // GET: /Driver/Vehicle/5
            public async Task<IActionResult> Vehicle(int id)
            {
                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var vehicle = await _context.Vehicles
                    .Include(v => v.ServiceRecords)
                        .ThenInclude(sr => sr.ServiceCenter)
                    .Include(v => v.ServiceRecords)
                        .ThenInclude(sr => sr.ServiceLineItems)
                            .ThenInclude(li => li.ServiceCategory)
                    .FirstOrDefaultAsync(v =>
                        v.Id == id &&
                        v.DriverId == userId);

                if (vehicle == null)
                {
                    return NotFound();
                }

                return View(vehicle);
            }

            // GET: /Driver/ServiceHistory/5
            public async Task<IActionResult> ServiceHistory(int id)
            {
                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v =>
                        v.Id == id &&
                        v.DriverId == userId);

                if (vehicle == null)
                {
                    return NotFound();
                }

                var serviceRecords = await _context.ServiceRecords
                    .Where(sr => sr.VehicleId == id)
                    .Include(sr => sr.ServiceCenter)
                    .Include(sr => sr.ServiceLineItems)
                        .ThenInclude(li => li.ServiceCategory)
                    .OrderByDescending(sr => sr.ServiceDate)
                    .ToListAsync();

                ViewBag.Vehicle = vehicle;

                return View(serviceRecords);
            }
        }
}
