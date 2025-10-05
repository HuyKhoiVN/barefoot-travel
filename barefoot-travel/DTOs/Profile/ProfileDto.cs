namespace barefoot_travel.DTOs.Profile
{
    public class ProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string? Photo { get; set; }
    }
}