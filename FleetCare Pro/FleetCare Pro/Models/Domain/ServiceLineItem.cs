using System.ComponentModel.DataAnnotations;


namespace FleetCare_Pro.Models.Domain
{
    public class ServiceLineItem
    {
        public int Id { get; set; }

        [Required]
        public int ServiceRecordId { get; set; }

        public ServiceRecord ServiceRecord { get; set; } = null!;

        [Required]
        public int ServiceCategoryId { get; set; }

        public ServiceCategory ServiceCategory { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }
    }
    
}
