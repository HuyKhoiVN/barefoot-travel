using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.Auth
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Role ID is required")]
        [Range(1, 2, ErrorMessage = "Role ID must be 1 (Admin) or 2 (User)")]
        public int RoleId { get; set; }

        public string? Photo { get; set; }

        [Required(ErrorMessage = "Active status is required")]
        public bool Active { get; set; }
    }
}
