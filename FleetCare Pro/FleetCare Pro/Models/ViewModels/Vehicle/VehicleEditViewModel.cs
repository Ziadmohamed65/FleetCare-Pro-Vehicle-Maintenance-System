using FleetCare_Pro.Models.Enums;
using FleetCare_Pro.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace FleetCare_Pro.Models.ViewModels.Vehicle
{
    public class VehicleEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(17, MinimumLength = 17)]
        [ValidVIN]
        public string VIN { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int Year { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        public VehicleStatus Status { get; set; }

        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        public string? DriverId { get; set; }

        public string? ExistingImageURL { get; set; }

        public IFormFile? VehicleImage { get; set; }
    }
}
