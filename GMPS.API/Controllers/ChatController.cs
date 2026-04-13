using GMPS.API.Hubs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepositories _chatRepo;
        private readonly ILogger<ChatController> _logger;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ChatController(
            IChatRepositories chatRepo,
            ILogger<ChatController> logger,
            IHubContext<ChatHub> chatHubContext)
        {
            _chatRepo = chatRepo ?? throw new ArgumentNullException(nameof(chatRepo));
            _logger = logger;
            _chatHubContext = chatHubContext ?? throw new ArgumentNullException(nameof(chatHubContext));
        }

        [HttpPost("/ai/openrouter/chat")]
        [Consumes("application/json")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult<ChatResponseDTO>> Send([FromBody] ChatRequestDTO request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Detail = "Tin nhan khong duoc de trong.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                _logger.LogInformation("Chatbox request: {Message}", request.Message);

                var result = await _chatRepo.SendMessageAsync(request);
                var chatMessage = new
                {
                    request.Message,
                    result.Reply
                };

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    await _chatHubContext.Clients
                        .Group(ChatHub.GetUserGroupName(userId))
                        .SendAsync("ChatMessageReceived", chatMessage);
                }
                else
                {
                    await _chatHubContext.Clients.All.SendAsync("ChatMessageReceived", chatMessage);
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Lỗi cấu hình AI");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status503ServiceUnavailable
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi kết nối AI API");
                return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                {
                    Detail = "Không thể kết nối tới dịch vụ AI. Vui lòng thử lại sau.",
                    Status = StatusCodes.Status502BadGateway
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định trong ChatController");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
