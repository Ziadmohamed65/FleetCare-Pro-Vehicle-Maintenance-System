using FleetCare_Pro.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetCare_Pro.ViewComponents
{
    public class OverdueMaintenanceViewComponent : ViewComponent
    {
        private readonly FleetCareDbContext _context;

        public OverdueMaintenanceViewComponent(FleetCareDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

            // Returns vehicles with NO service records OR where all service records are older than 6 months
            var vehicles = await _context.Vehicles
            .Include(v => v.ServiceRecords)
            .Where(v => !v.ServiceRecords.Any(sr => sr.ServiceDate >= sixMonthsAgo))
            .ToListAsync();

            return View(vehicles);
        }
    }
}