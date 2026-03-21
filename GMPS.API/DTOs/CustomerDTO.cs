namespace GMPS.API.DTOs
{
    public class CustomerDTO
    {
        public int Id { get; set; }

        public string? UserName { get; set; }
        public string? AvatarUrl { get; set; }

        public string FullName { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }
    }
}
