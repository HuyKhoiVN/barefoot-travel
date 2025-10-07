using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.Policy
{
    public class PolicyDto
    {
        public int Id { get; set; }
        public string PolicyType { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public bool Active { get; set; }
    }

    public class CreatePolicyDto
    {
        [Required(ErrorMessage = "Policy type is required")]
        [StringLength(100, ErrorMessage = "Policy type cannot exceed 100 characters")]
        public string PolicyType { get; set; } = string.Empty;
    }

    public class UpdatePolicyDto
    {
        [Required(ErrorMessage = "Policy type is required")]
        [StringLength(100, ErrorMessage = "Policy type cannot exceed 100 characters")]
        public string PolicyType { get; set; } = string.Empty;
    }

    public class PolicyStatusDto
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
