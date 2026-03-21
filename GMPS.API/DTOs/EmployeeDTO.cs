namespace GMPS.API.DTOs
{
    public class EmployeeDTO
    {
        public int Id { get; set; }

        public string? UserName { get; set; }
        public string? AvatarUrl { get; set; }

        public string FullName { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string Role { get; set; } = null!;

        public string WorkerRole { get; set; } = null!;

        public string Status { get; set; } = null!;
    }
}
