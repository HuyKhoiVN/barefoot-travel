namespace barefoot_travel.DTOs.Auth
{
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int RoleId { get; set; } = 2; // Default to User role
    }
}
