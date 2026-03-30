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

        [HttpPost("send")]
        [Authorize]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult<ChatResponseDTO>> Send([FromBody] ChatRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ValidationProblemDetails(ModelState)
                    {
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Detail = "Tin nhắn không được để trống.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }
                var userRole = User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                var userName = User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Unknown";

                _logger.LogInformation("Chatbox request từ user [{UserName}] với role [{Role}]: {Message}",
                    userName, userRole ?? "none", request.Message);

                var result = await _chatRepo.SendMessageAsync(request, userRole);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Lỗi cấu hình OpenAI");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.4"
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi kết nối OpenAI API");
                return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                {
                    Detail = "Không thể kết nối tới dịch vụ AI. Vui lòng thử lại sau.",
                    Status = StatusCodes.Status502BadGateway,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.3"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định trong ChatController");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }

        [HttpPost("guest")]
        [AllowAnonymous]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult<ChatResponseDTO>> GuestSend([FromBody] ChatRequestDTO request)
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

                _logger.LogInformation("Chatbox request từ khách vãng lai: {Message}", request.Message);

                var result = await _chatRepo.SendMessageAsync(request, null);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Lỗi cấu hình OpenAI (guest)");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status503ServiceUnavailable
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi kết nối OpenAI API (guest)");
                return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status502BadGateway                   
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định trong ChatController (guest)");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
