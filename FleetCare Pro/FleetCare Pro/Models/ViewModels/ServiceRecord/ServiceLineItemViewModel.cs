using System.ComponentModel.DataAnnotations;



namespace FleetCare_Pro.Models.ViewModels.ServiceRecord
{

     public class ServiceLineItemViewModel
     {
         [Required]
         public int ServiceCategoryId { get; set; }

         [Required]
         [StringLength(500)]
         public string Description { get; set; } = string.Empty;

         [Range(0, double.MaxValue)]
         public decimal Cost { get; set; }
     }


}
