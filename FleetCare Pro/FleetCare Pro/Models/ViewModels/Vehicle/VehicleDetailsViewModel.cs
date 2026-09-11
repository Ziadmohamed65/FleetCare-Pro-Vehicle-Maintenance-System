using FleetCare_Pro.Models.Enums;

namespace FleetCare_Pro.Models.ViewModels.Vehicle
{
    public class VehicleDetailsViewModel
    {
        public int Id { get; set; }

        public string VIN { get; set; } = string.Empty;

        public string LicensePlate { get; set; } = string.Empty;

        public string Make { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal PurchasePrice { get; set; }

        public VehicleStatus Status { get; set; }

        public int Mileage { get; set; }

        public string? VehicleImageURL { get; set; }

        public string? DriverName { get; set; }

        public int ServiceRecordCount { get; set; }
    }
}
