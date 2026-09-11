using FleetCare_Pro.Data;
using FleetCare_Pro.Filters;
using FleetCare_Pro.Models.Domain;
using FleetCare_Pro.Models.Identity;
using FleetCare_Pro.Models.ViewModels.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetCare_Pro.Controllers
{


    public class VehicleController : Controller
    {
        private readonly FleetCareDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public VehicleController(FleetCareDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Driver)
                .ToListAsync();

            return View(vehicles);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Driver)
                .Include(v => v.ServiceRecords)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            var viewModel = new VehicleDetailsViewModel
            {
                Id = vehicle.Id,
                VIN = vehicle.VIN,
                LicensePlate = vehicle.LicensePlate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                PurchasePrice = vehicle.PurchasePrice,
                Status = vehicle.Status,
                Mileage = vehicle.Mileage,
                VehicleImageURL = vehicle.VehicleImageURL,
                DriverName = vehicle.Driver?.FullName,
                ServiceRecordCount = vehicle.ServiceRecords.Count
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Drivers = await _userManager.GetUsersInRoleAsync("Driver");

            return View(new VehicleCreateViewModel());
        }

        [AuditLog("Create Vehicle")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleCreateViewModel CreateModel)
        {
            ViewBag.Drivers =
                await _userManager.GetUsersInRoleAsync("Driver");

            if (!ModelState.IsValid)
            {
                return View(CreateModel);
            }

            bool vinExists = await _context.Vehicles
                .AnyAsync(v => v.VIN == CreateModel.VIN);

            if (vinExists)
            {
                ModelState.AddModelError(
                    nameof(CreateModel.VIN),
                    "A vehicle with this VIN already exists.");

                return View(CreateModel);
            }

            var vehicle = new Vehicle
            {
                VIN = CreateModel.VIN.ToUpperInvariant(),
                LicensePlate = CreateModel.LicensePlate,
                Make = CreateModel.Make,
                Model = CreateModel.Model,
                Year = CreateModel.Year,
                PurchasePrice = CreateModel.PurchasePrice,
                Status = CreateModel.Status,
                Mileage = CreateModel.Mileage,
                DriverId = CreateModel.DriverId
            };

            if (CreateModel.VehicleImage != null)
            {
                vehicle.VehicleImageURL =
                    await SaveVehicleImage(CreateModel.VehicleImage);
            }

            _context.Vehicles.Add(vehicle);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Vehicle created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            ViewBag.Drivers = await _userManager.GetUsersInRoleAsync("Driver");

            var model = new VehicleEditViewModel
            {
                Id = vehicle.Id,
                VIN = vehicle.VIN,
                LicensePlate = vehicle.LicensePlate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                PurchasePrice = vehicle.PurchasePrice,
                Status = vehicle.Status,
                Mileage = vehicle.Mileage,
                DriverId = vehicle.DriverId,
                ExistingImageURL = vehicle.VehicleImageURL
            };

            return View(model);
        }

        [AuditLog("Update Vehicle")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleEditViewModel EditModel)
        {
            ViewBag.Drivers = await _userManager.GetUsersInRoleAsync("Driver");

            if (id != EditModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(EditModel);
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            // Check duplicate VIN
            bool vinExists = await _context.Vehicles
                .AnyAsync(v =>
                    v.VIN == EditModel.VIN &&
                    v.Id != id);

            if (vinExists)
            {
                ModelState.AddModelError(
                    nameof(EditModel.VIN),
                    "A vehicle with this VIN already exists.");

                return View(EditModel);
            }

            vehicle.VIN = EditModel.VIN.ToUpperInvariant();
            vehicle.LicensePlate = EditModel.LicensePlate;
            vehicle.Make = EditModel.Make;
            vehicle.Model = EditModel.Model;
            vehicle.Year = EditModel.Year;
            vehicle.PurchasePrice = EditModel.PurchasePrice;
            vehicle.Status = EditModel.Status;
            vehicle.Mileage = EditModel.Mileage;
            vehicle.DriverId = EditModel.DriverId;

            // Replace image if a new one was uploaded
            if (EditModel.VehicleImage != null)
            {
                DeleteVehicleImage(vehicle.VehicleImageURL);

                vehicle.VehicleImageURL =
                    await SaveVehicleImage(EditModel.VehicleImage);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Vehicle updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Driver)
                .Include(v => v.ServiceRecords)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            var viewModel = new VehicleDetailsViewModel
            {
                Id = vehicle.Id,
                VIN = vehicle.VIN,
                LicensePlate = vehicle.LicensePlate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                PurchasePrice = vehicle.PurchasePrice,
                Status = vehicle.Status,
                Mileage = vehicle.Mileage,
                VehicleImageURL = vehicle.VehicleImageURL,
                DriverName = vehicle.Driver?.FullName,
                ServiceRecordCount = vehicle.ServiceRecords.Count
            };

            return View(viewModel); 
        }

        [AuditLog("Delete Vehicle")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            DeleteVehicleImage(vehicle.VehicleImageURL);

            _context.Vehicles.Remove(vehicle);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Vehicle deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveVehicleImage(IFormFile image)
        {
            string uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "vehicles");

            Directory.CreateDirectory(uploadsFolder);

            string extension =
                Path.GetExtension(image.FileName);

            string fileName =
                $"{Guid.NewGuid()}{extension}";

            string filePath =
                Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(
                filePath,
                FileMode.Create);

            await image.CopyToAsync(stream);

            return $"/uploads/vehicles/{fileName}";
        }

        private void DeleteVehicleImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            string fileName =
                Path.GetFileName(imageUrl);

            string filePath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "vehicles",
                fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}