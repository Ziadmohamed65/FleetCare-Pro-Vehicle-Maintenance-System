using System.ComponentModel.DataAnnotations;


namespace FleetCare_Pro.Models.ViewModels.ServiceCategory
{
  
     public class ServiceCategoryEditViewModel
     {
         public int Id { get; set; }

         [Required]
         [StringLength(100)]
         public string CategoryName { get; set; } = string.Empty;

         [StringLength(500)]
         public string? Description { get; set; }

         [Range(1, 120)]
         [Display(Name = "Recommended Interval (Months)")]
         public int RecommendedIntervalMonths { get; set; }
    }
}

