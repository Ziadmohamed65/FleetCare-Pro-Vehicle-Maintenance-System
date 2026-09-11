using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using FleetCare_Pro.Models.Domain;
namespace FleetCare_Pro.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EmployeeId { get; set; } = string.Empty;

        public ICollection<Vehicle> AssignedVehicles { get; set; }
            = new List<Vehicle>();

        public ICollection<ServiceRecord> ServiceRecordsCreated { get; set; }
            = new List<ServiceRecord>();
    }
}
