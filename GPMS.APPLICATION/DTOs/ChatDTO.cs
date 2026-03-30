namespace GPMS.APPLICATION.DTOs
{
    public class ChatRequestDTO
    {
        public string Message { get; set; } = string.Empty;
    }
    public class ChatResponseDTO
    {
        public string Reply { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
