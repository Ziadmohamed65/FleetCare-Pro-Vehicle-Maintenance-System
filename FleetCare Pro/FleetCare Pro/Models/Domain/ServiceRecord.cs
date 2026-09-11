using FleetCare_Pro.Models.Enums;
using FleetCare_Pro.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace FleetCare_Pro.Models.Domain
{
        public class ServiceRecord
        {
            public int Id { get; set; }

            [Required]
            public int VehicleId { get; set; }

            public Vehicle Vehicle { get; set; } = null!;

            [Required]
            public int ServiceCenterId { get; set; }

            public ServiceCenter ServiceCenter { get; set; } = null!;

            [Required]
            public DateTime ServiceDate { get; set; }

            [Range(0, int.MaxValue)]
            public int CurrentMileage { get; set; }

            [Range(0, double.MaxValue)]
            public decimal TotalCost { get; set; }

            public string? InvoiceDocumentPath { get; set; }

            [StringLength(1000)]
            public string? Notes { get; set; }

            public ServiceStatus Status { get; set; }

            [Required]
            public string CreatedByUserId { get; set; } = string.Empty;

            public ApplicationUser CreatedByUser { get; set; } = null!;

            public ICollection<ServiceLineItem> ServiceLineItems { get; set; }
                = new List<ServiceLineItem>();
        }
    
}
