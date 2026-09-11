using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FleetCare_Pro.Models.ViewModels.ServiceRecord
{
     public class ServiceRecordCreateViewModel
     {
         [Required]
         [Display(Name = "Vehicle")]
         public int VehicleId { get; set; }

         [Required]
         [Display(Name = "Service Center")]
         public int ServiceCenterId { get; set; }

         [Required]
         [DataType(DataType.Date)]
         [Display(Name = "Service Date")]
         public DateTime ServiceDate { get; set; } = DateTime.Today;

         [Range(0, int.MaxValue)]
         [Display(Name = "Current Mileage")]
         public int CurrentMileage { get; set; }

         [StringLength(1000)]
         public string? Notes { get; set; }

         public IFormFile? Invoice { get; set; }

         public List<ServiceLineItemViewModel> ServiceLineItems { get; set; }
             = new List<ServiceLineItemViewModel>();
     }

}
