namespace GMPS.API.DTOs
{
    public class PermissionResponseDTO
    {
        public int Id { get; set; }
        public string Controller { get; set; } = null!;
        public string Method { get; set; } = null!;
        public string Action { get; set; } = null!;
        public List<RoleDTO> Roles { get; set; } = new();
    }
}
