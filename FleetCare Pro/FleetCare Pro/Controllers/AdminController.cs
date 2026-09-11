using FleetCare_Pro.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetCare_Pro.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
   {
       private readonly FleetCareDbContext _context;

       public AdminController(FleetCareDbContext context)
       {
           _context = context;
       }

       // GET: /Admin/Index
       public async Task<IActionResult> Index()
       {
           var vehicleCount = await _context.Vehicles.CountAsync();

           var serviceCenterCount =
               await _context.ServiceCenters.CountAsync();

           var serviceCategoryCount =
               await _context.ServiceCategories.CountAsync();

           var serviceRecordCount =
               await _context.ServiceRecords.CountAsync();

           var pendingServiceRecords =
               await _context.ServiceRecords
                   .CountAsync(sr =>
                       sr.Status == Models.Enums.ServiceStatus.Pending);

           var completedServiceRecords =
               await _context.ServiceRecords
                   .CountAsync(sr =>
                       sr.Status == Models.Enums.ServiceStatus.Completed);

           var totalMaintenanceCost =
               await _context.ServiceRecords
                   .SumAsync(sr => (decimal?)sr.TotalCost) ?? 0;

           ViewBag.VehicleCount = vehicleCount;
           ViewBag.ServiceCenterCount = serviceCenterCount;
           ViewBag.ServiceCategoryCount = serviceCategoryCount;
           ViewBag.ServiceRecordCount = serviceRecordCount;
           ViewBag.PendingServiceRecords = pendingServiceRecords;
           ViewBag.CompletedServiceRecords = completedServiceRecords;
           ViewBag.TotalMaintenanceCost = totalMaintenanceCost;

           return View();
       }
   }
}
