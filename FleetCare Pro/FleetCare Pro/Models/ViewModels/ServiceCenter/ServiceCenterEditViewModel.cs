using System.ComponentModel.DataAnnotations;



namespace FleetCare_Pro.Models.ViewModels.ServiceCenter
{
    public class ServiceCenterEditViewModel
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
    }

}
