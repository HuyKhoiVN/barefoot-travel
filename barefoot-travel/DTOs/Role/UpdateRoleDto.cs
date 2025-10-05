namespace barefoot_travel.DTOs.Role
{
    public class UpdateRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; } = true;
    }
}
