using FleetCare_Pro.Models.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;



namespace FleetCare_Pro.Models.ViewModels.ServiceRecord
{
    public class ServiceRecordEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Required]
        [Display(Name = "Service Center")]
        public int ServiceCenterId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Service Date")]
        public DateTime ServiceDate { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Current Mileage")]
        public int CurrentMileage { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public ServiceStatus Status { get; set; }

        public string? ExistingInvoiceDocumentPath { get; set; }

        public IFormFile? Invoice { get; set; }

        public List<ServiceLineItemViewModel> ServiceLineItems { get; set; }
            = new List<ServiceLineItemViewModel>();
    }
    
}
