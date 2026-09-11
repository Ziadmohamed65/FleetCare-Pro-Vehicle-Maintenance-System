using System.ComponentModel.DataAnnotations;

namespace FleetCare_Pro.Models.Domain
{
    public class ServiceCenter
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public ICollection<ServiceRecord> ServiceRecords { get; set; }
            = new List<ServiceRecord>();
    }
}
