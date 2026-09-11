using FleetCare_Pro.Data;
using FleetCare_Pro.Models.Domain;
using FleetCare_Pro.Models.Enums;
using FleetCare_Pro.Models.ViewModels.ServiceRecord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetCare_Pro.Controllers
{
        public class ServiceRecordController : Controller
        {
            private readonly FleetCareDbContext _context;
            private readonly IWebHostEnvironment _environment;
            private readonly ILogger<ServiceRecordController> _logger;

            private const long MaxInvoiceSize = 5 * 1024 * 1024;

            private static readonly string[] AllowedInvoiceExtensions =
            {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png"
        };

            public ServiceRecordController(
                FleetCareDbContext context,
                IWebHostEnvironment environment,
                ILogger<ServiceRecordController> logger)
            {
                _context = context;
                _environment = environment;
                _logger = logger;
            }

            public async Task<IActionResult> Index()
            {
                var records = await _context.ServiceRecords
                    .Include(sr => sr.Vehicle)
                    .Include(sr => sr.ServiceCenter)
                    .Include(sr => sr.CreatedByUser)
                    .OrderByDescending(sr => sr.ServiceDate)
                    .ToListAsync();

                return View(records);
            }
        [HttpGet]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> MyServices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var records = await _context.ServiceRecords
                .Include(sr => sr.Vehicle)
                .Include(sr => sr.ServiceCenter)
                .Include(sr => sr.ServiceLineItems)
                    .ThenInclude(li => li.ServiceCategory)
                .Where(sr => sr.Vehicle.DriverId == userId)
                .OrderByDescending(sr => sr.ServiceDate)
                .ToListAsync();

            return View(records);
        }


        public async Task<IActionResult> Details(int id)
            {
                var record = await _context.ServiceRecords
                    .Include(sr => sr.Vehicle)
                    .Include(sr => sr.ServiceCenter)
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.ServiceLineItems)
                        .ThenInclude(li => li.ServiceCategory)
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                if (record == null)
                {
                    return NotFound();
                }
              if (User.IsInRole("Driver"))
              {
                  var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                  if (record.Vehicle.DriverId != userId)
                      return Forbid();
              }

            return View(record);
            }



            [HttpGet]
            public async Task<IActionResult> Create()
            {
                await LoadDropdownsAsync();

                var model = new ServiceRecordCreateViewModel
                {
                    ServiceDate = DateTime.Today,
                    ServiceLineItems = new List<ServiceLineItemViewModel>
                {
                    new ServiceLineItemViewModel()
                }
                };

                return View(model);
            }



            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(ServiceRecordCreateViewModel model)
            {
                if (model.ServiceLineItems == null ||
                    model.ServiceLineItems.Count == 0)
                {
                    ModelState.AddModelError(
                        "ServiceLineItems",
                        "At least one service item is required.");
                }

                if (model.Invoice != null)
                {
                    ValidateInvoice(model.Invoice);
                }

                if (!ModelState.IsValid)
                {
                    await LoadDropdownsAsync();
                    return View(model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(
                        "",
                        "You must be logged in to create a service record.");

                    await LoadDropdownsAsync();
                    return View(model);
                }

                var vehicleExists = await _context.Vehicles
                    .AnyAsync(v => v.Id == model.VehicleId);

                var serviceCenterExists = await _context.ServiceCenters
                    .AnyAsync(sc =>
                        sc.Id == model.ServiceCenterId &&
                        sc.IsActive);

                if (!vehicleExists)
                {
                    ModelState.AddModelError(
                        "VehicleId",
                        "Selected vehicle does not exist.");
                }

                if (!serviceCenterExists)
                {
                    ModelState.AddModelError(
                        "ServiceCenterId",
                        "Selected service center does not exist or is inactive.");
                }

                var categoryIds = model.ServiceLineItems
                    .Select(x => x.ServiceCategoryId)
                    .Distinct()
                    .ToList();

                var validCategoryIds = await _context.ServiceCategories
                    .Where(c => categoryIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();

                if (validCategoryIds.Count != categoryIds.Count)
                {
                    ModelState.AddModelError(
                        "ServiceLineItems",
                        "One or more selected service categories are invalid.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadDropdownsAsync();
                    return View(model);
                }

                string? invoicePath = null;

                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    // Save invoice
                    if (model.Invoice != null)
                    {
                        invoicePath = await SaveInvoiceAsync(model.Invoice);
                    }

                    // Calculate total on server
                    decimal totalCost = model.ServiceLineItems
                        .Sum(item => item.Cost);

                    var serviceRecord = new ServiceRecord
                    {
                        VehicleId = model.VehicleId,
                        ServiceCenterId = model.ServiceCenterId,
                        ServiceDate = model.ServiceDate,
                        CurrentMileage = model.CurrentMileage,
                        TotalCost = totalCost,
                        InvoiceDocumentPath = invoicePath,
                        Notes = model.Notes,
                        Status = ServiceStatus.Pending,
                        CreatedByUserId = userId
                    };

                    _context.ServiceRecords.Add(serviceRecord);

                    await _context.SaveChangesAsync();

                    // Add detail records
                    foreach (var item in model.ServiceLineItems)
                    {
                        var lineItem = new ServiceLineItem
                        {
                            ServiceRecordId = serviceRecord.Id,
                            ServiceCategoryId = item.ServiceCategoryId,
                            Description = item.Description,
                            Cost = item.Cost
                        };

                        _context.ServiceLineItems.Add(lineItem);
                    }

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Success"] =
                        "Service record created successfully.";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // Database transaction cannot delete files automatically.
                    if (!string.IsNullOrEmpty(invoicePath))
                    {
                        DeleteInvoiceFile(invoicePath);
                    }

                    _logger.LogError(
                        ex,
                        "Error creating service record.");

                    ModelState.AddModelError(
                        "",
                        "An error occurred while creating the service record.");

                    await LoadDropdownsAsync();

                    return View(model);
                }
            }



            [HttpGet]
            public async Task<IActionResult> Edit(int id)
            {
                var record = await _context.ServiceRecords
                    .Include(sr => sr.ServiceLineItems)
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                if (record == null)
                {
                    return NotFound();
                }

                var model = new ServiceRecordEditViewModel
                {
                    Id = record.Id,
                    VehicleId = record.VehicleId,
                    ServiceCenterId = record.ServiceCenterId,
                    ServiceDate = record.ServiceDate,
                    CurrentMileage = record.CurrentMileage,
                    Notes = record.Notes,
                    Status = record.Status,
                    ExistingInvoiceDocumentPath =
                        record.InvoiceDocumentPath,

                    ServiceLineItems = record.ServiceLineItems
                        .Select(li => new ServiceLineItemViewModel
                        {
                            ServiceCategoryId = li.ServiceCategoryId,
                            Description = li.Description,
                            Cost = li.Cost
                        })
                        .ToList()
                };

                if (model.ServiceLineItems.Count == 0)
                {
                    model.ServiceLineItems.Add(
                        new ServiceLineItemViewModel());
                }

                await LoadDropdownsAsync();

                return View(model);
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(
                ServiceRecordEditViewModel model)
            {
                if (model.ServiceLineItems == null ||
                    model.ServiceLineItems.Count == 0)
                {
                    ModelState.AddModelError(
                        "ServiceLineItems",
                        "At least one service item is required.");
                }

                if (model.Invoice != null)
                {
                    ValidateInvoice(model.Invoice);
                }

                if (!ModelState.IsValid)
                {
                    await LoadDropdownsAsync();
                    return View(model);
                }

                var record = await _context.ServiceRecords
                    .Include(sr => sr.ServiceLineItems)
                    .FirstOrDefaultAsync(sr => sr.Id == model.Id);

                if (record == null)
                {
                    return NotFound();
                }

                string? newInvoicePath = null;
                string? oldInvoicePath = record.InvoiceDocumentPath;

                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    // New invoice
                    if (model.Invoice != null)
                    {
                        newInvoicePath =
                            await SaveInvoiceAsync(model.Invoice);

                        record.InvoiceDocumentPath = newInvoicePath;
                    }

                    record.VehicleId = model.VehicleId;
                    record.ServiceCenterId = model.ServiceCenterId;
                    record.ServiceDate = model.ServiceDate;
                    record.CurrentMileage = model.CurrentMileage;
                    record.Notes = model.Notes;
                    record.Status = model.Status;

                    // Recalculate total
                    record.TotalCost = model.ServiceLineItems
                        .Sum(item => item.Cost);

                    // Remove old details
                    _context.ServiceLineItems.RemoveRange(
                        record.ServiceLineItems);

                    // Add new details
                    foreach (var item in model.ServiceLineItems)
                    {
                        record.ServiceLineItems.Add(
                            new ServiceLineItem
                            {
                                ServiceRecordId = record.Id,
                                ServiceCategoryId = item.ServiceCategoryId,
                                Description = item.Description,
                                Cost = item.Cost
                            });
                    }

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Delete old invoice only after successful DB update
                    if (!string.IsNullOrEmpty(newInvoicePath) &&
                        !string.IsNullOrEmpty(oldInvoicePath))
                    {
                        DeleteInvoiceFile(oldInvoicePath);
                    }

                    TempData["Success"] =
                        "Service record updated successfully.";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    if (!string.IsNullOrEmpty(newInvoicePath))
                    {
                        DeleteInvoiceFile(newInvoicePath);
                    }

                    _logger.LogError(
                        ex,
                        "Error updating service record {Id}",
                        model.Id);

                    ModelState.AddModelError(
                        "",
                        "An error occurred while updating the service record.");

                    await LoadDropdownsAsync();

                    return View(model);
                }
            }


            [HttpGet]
            public async Task<IActionResult> Delete(int id)
            {
                var record = await _context.ServiceRecords
                    .Include(sr => sr.Vehicle)
                    .Include(sr => sr.ServiceCenter)
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                if (record == null)
                {
                    return NotFound();
                }

                return View(record);
            }


            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var record = await _context.ServiceRecords
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                if (record == null)
                {
                    return NotFound();
                }

                var invoicePath = record.InvoiceDocumentPath;

                try
                {
                    _context.ServiceRecords.Remove(record);

                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(invoicePath))
                    {
                        DeleteInvoiceFile(invoicePath);
                    }

                    TempData["Success"] =
                        "Service record deleted successfully.";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error deleting service record {Id}",
                        id);

                    TempData["Error"] =
                        "Unable to delete the service record.";

                    return RedirectToAction(nameof(Index));
                }
            }


            private async Task LoadDropdownsAsync()
            {
                var vehicles = await _context.Vehicles
                    .OrderBy(v => v.Make)
                    .ThenBy(v => v.Model)
                    .ToListAsync();

                ViewBag.Vehicles = vehicles
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text =
                            $"{v.Make} {v.Model} - {v.LicensePlate}"
                    })
                    .ToList();

                var serviceCenters = await _context.ServiceCenters
                    .Where(sc => sc.IsActive)
                    .OrderBy(sc => sc.Name)
                    .ToListAsync();

                ViewBag.ServiceCenters = serviceCenters
                    .Select(sc => new SelectListItem
                    {
                        Value = sc.Id.ToString(),
                        Text = sc.Name
                    })
                    .ToList();

                var categories = await _context.ServiceCategories
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                ViewBag.ServiceCategories = categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.CategoryName
                    })
                    .ToList();
            }


            private void ValidateInvoice(IFormFile invoice)
            {
                if (invoice.Length == 0)
                {
                    ModelState.AddModelError(
                        "Invoice",
                        "The invoice file is empty.");

                    return;
                }

                if (invoice.Length > MaxInvoiceSize)
                {
                    ModelState.AddModelError(
                        "Invoice",
                        "Invoice file must be 5 MB or smaller.");

                    return;
                }

                var extension =
                    Path.GetExtension(invoice.FileName)
                        .ToLowerInvariant();

                if (!AllowedInvoiceExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "Invoice",
                        "Only PDF, JPG, JPEG, and PNG files are allowed.");
                }
            }


            private async Task<string> SaveInvoiceAsync(IFormFile invoice)
            {
                var uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "invoices");

                Directory.CreateDirectory(uploadsFolder);

                var extension =
                    Path.GetExtension(invoice.FileName)
                        .ToLowerInvariant();

                var fileName =
                    $"{Guid.NewGuid()}{extension}";

                var filePath =
                    Path.Combine(uploadsFolder, fileName);

                await using var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create);

                await invoice.CopyToAsync(stream);

                return $"/uploads/invoices/{fileName}";
            }


            private void DeleteInvoiceFile(string invoicePath)
            {
                var relativePath = invoicePath
                    .TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar);

                var filePath = Path.Combine(
                    _environment.WebRootPath,
                    relativePath);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

}
