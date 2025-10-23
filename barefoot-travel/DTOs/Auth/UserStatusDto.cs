using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.Auth
{
    public class UserStatusDto
    {
        [Required(ErrorMessage = "Active status is required")]
        public bool Active { get; set; }
    }
}
