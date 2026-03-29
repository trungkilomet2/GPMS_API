using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer,Owner")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepositories _commentRepo;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentRepositories commentInterface, ILogger<CommentController> logger)
        {
            _commentRepo = commentInterface ?? throw new ArgumentNullException(nameof(commentInterface));
            _logger = logger;
        }

        [HttpGet("get-comment-by-orderId/{orderId}")]
        public async Task<ActionResult<RestDTO<IEnumerable<CommentDTO>>>> GetCommentByOrderId(int orderId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);           
            try
            {
                _logger.LogInformation(CustomLogEvents.CommentController_Get,
                    "Đang lấy comment cho OrderId {OrderId}", orderId);
                var result = await _commentRepo.GetCommentByOrderId(orderId);
                if (!result.Any())
                    {
                    _logger.LogInformation(CustomLogEvents.CommentController_Get,
                        "Không tìm thấy comment cho OrderId {OrderId}", orderId);
                    return StatusCode(StatusCodes.Status404NotFound, $"Không tìm thấy Comment cho OrderId '{orderId}'");
                }
                var comment = result.Select(c => new CommentDTO
                {
                    Id = c.Id,
                    ToOrderId = c.toOrderId,
                    Content = c.Content,
                    SendDateTime = c.SendDateTime
                });

                _logger.LogInformation(CustomLogEvents.CommentController_Get,
                    "Trả về {Count} comments cho OrderId {OrderId}", result.Count(), orderId);

                var response = new RestDTO<IEnumerable<CommentDTO>>
                {
                    Data = comment,
                    RecordCount = result.Count(),
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "Comment", null, Request.Scheme)!, "self", "GET")
                    }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                _logger.LogError(CustomLogEvents.Error_Get, ex,
                    "Lỗi khi lấy comments cho OrderId {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPost("create-comment")]
        public async Task<ActionResult> CreateComment([FromBody] CreatedCommentDTO? comment)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation(CustomLogEvents.CommentController_Post,
                        "Tạo comment cho OrderId {OrderId} by UserId {UserId}",
                        comment.ToOrderId, comment.FromUserId);
                    var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    var newComment = new Comment
                    {
                        fromUserId = comment.FromUserId,
                        toOrderId = comment.ToOrderId,
                        Content = comment.Content,
                        SendDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
                    };

                    var result = await _commentRepo.CreateComment(userId,newComment);

                    _logger.LogInformation(CustomLogEvents.CommentController_Post,
                        "Comment {CommentId} được tạo thành công cho OrderId {OrderId}",
                        result.Id, comment.ToOrderId);

                    return StatusCode(StatusCodes.Status201Created,
                        $"Comment '{result.Id}' đã được tạo thành công");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.CommentController_Post,
                        "Lỗi model state khi tạo comment");

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    };

                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.CommentController_Post, ex,
                    "Lỗi khi tạo comment");

                var exceptionDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPut("update-comment/{CommentId}")]
        public async Task<ActionResult<Comment>> UpdateComment(int CommentId, [FromBody] UpdatedCommentDTO? comment)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                if (ModelState.IsValid)
                {
                    var commentExists = await _commentRepo.GetCommentById(CommentId);
                    if(commentExists == null)
                    {
                        _logger.LogWarning(CustomLogEvents.CommentController_Put,
                            "Comment {CommentId} not found for update", CommentId);
                        return StatusCode(StatusCodes.Status404NotFound, $"Comment '{CommentId}' not found");
                    }
                    _logger.LogInformation(CustomLogEvents.CommentController_Put,
                        "Updating comment {CommentId}", CommentId);
                    var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    var newComment = new Comment
                    {
                        Id = CommentId,
                        Content = comment.Content,
                        SendDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
                    };

                    var updated = await _commentRepo.UpdateComment(newComment, userId);

                    _logger.LogInformation(CustomLogEvents.CommentController_Put,
                        "Comment {CommentId} updated successfully", CommentId);

                    return StatusCode(StatusCodes.Status200OK, updated);
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.CommentController_Put,
                        "Invalid model state when updating comment {CommentId}", CommentId);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };

                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.CommentController_Put, ex,
                    "Error updating comment {CommentId}", CommentId);

                var exceptionDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpDelete("delete-comment/{CommentId}")]
        public async Task<ActionResult> DeleteComment(int CommentId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                var commentExists = await _commentRepo.GetCommentById(CommentId);
                if (commentExists == null)
                {
                    _logger.LogWarning(CustomLogEvents.CommentController_Put,
                        "Comment {CommentId} not found for update", CommentId);
                    return StatusCode(StatusCodes.Status404NotFound, $"Comment '{CommentId}' not found");
                }
                _logger.LogInformation(CustomLogEvents.CommentController_Delete,
                    "Deleting comment {CommentId}", CommentId);

                await _commentRepo.DeleteComment(CommentId, userId);

                _logger.LogInformation(CustomLogEvents.CommentController_Delete,
                    "Comment {CommentId} deleted successfully", CommentId);

                return StatusCode(StatusCodes.Status200OK, $"Comment '{CommentId}' has been deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.CommentController_Delete, ex,
                    "Error deleting comment {CommentId}", CommentId);

                var exceptionDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }
    }
}
