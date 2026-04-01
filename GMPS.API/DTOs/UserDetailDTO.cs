namespace GMPS.API.DTOs
{
    public class UserDetailDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public string WorkerRole { get; set; } = null!;
    }
}
