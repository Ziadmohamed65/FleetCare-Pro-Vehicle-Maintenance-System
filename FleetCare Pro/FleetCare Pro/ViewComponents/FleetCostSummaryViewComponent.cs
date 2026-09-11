using FleetCare_Pro.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetCare_Pro.ViewComponents
{
   
   public class FleetCostSummaryViewComponent : ViewComponent
   {
       private readonly FleetCareDbContext _context;

       public FleetCostSummaryViewComponent(FleetCareDbContext context)
       {
           _context = context;
       }

       public async Task<IViewComponentResult> InvokeAsync()
       {
           var totalMaintenanceCost = await _context.ServiceRecords
               .SumAsync(sr => (decimal?)sr.TotalCost) ?? 0m;

           return View(totalMaintenanceCost);
       }
   }

}
