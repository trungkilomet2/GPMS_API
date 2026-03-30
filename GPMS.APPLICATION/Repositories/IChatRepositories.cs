using GPMS.APPLICATION.DTOs;

namespace GPMS.APPLICATION.Repositories
{
    public interface IChatRepositories
    {
        Task<ChatResponseDTO> SendMessageAsync(ChatRequestDTO request, string? userRole);
    }
}
