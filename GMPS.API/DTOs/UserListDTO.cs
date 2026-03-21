namespace GMPS.API.DTOs
{
    public class UserListDTO
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvartarUrl { get; set; }
        public string? Location { get; set; }
        public string? Email { get; set; }
        public int StatusId { get; set; }
    }
}
