using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepositories _chatRepo;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatRepositories chatRepo, ILogger<ChatController> logger)
        {
            _chatRepo = chatRepo ?? throw new ArgumentNullException(nameof(chatRepo));
            _logger = logger;
        }

        [HttpPost("/ai/gemini/chat")]
        [AllowAnonymous]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult<ChatResponseDTO>> Send([FromBody] ChatRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Detail = "Tin nhắn không được để trống.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                _logger.LogInformation("Chatbox request: {Message}", request.Message);

                var result = await _chatRepo.SendMessageAsync(request);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Lỗi cấu hình Gemini");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status503ServiceUnavailable
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi kết nối Gemini API");
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
