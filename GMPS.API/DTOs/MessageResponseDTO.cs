namespace GMPS.API.DTOs
{
    public class MessageResponseDTO<T>
    {
        public string MessageCode { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
