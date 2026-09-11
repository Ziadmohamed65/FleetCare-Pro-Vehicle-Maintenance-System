using System.ComponentModel.DataAnnotations;

namespace FleetCare_Pro.Models.Domain
{
    public class ServiceCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 120)]
        public int RecommendedIntervalMonths { get; set; }

        public ICollection<ServiceLineItem> ServiceLineItems { get; set; }
            = new List<ServiceLineItem>();
    }
}
