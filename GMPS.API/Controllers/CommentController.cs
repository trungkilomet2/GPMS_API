using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUserRepositories _userRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentRepositories commentInterface, IConfiguration configuration, ILogger<CommentController> logger, IUserRepositories userRepo)
        {
            _commentRepo = commentInterface ?? throw new ArgumentNullException(nameof(commentInterface));
            _configuration = configuration;
            _logger = logger;
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        [HttpGet("get-comment-by-orderId/{orderId}")]
        public async Task<IActionResult> GetCommentByOrderId(int orderId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userRepo.GetUserById(userId);
            try
            {
                _logger.LogInformation(CustomLogEvents.CommentController_Get,
                    "Getting comments for OrderId {OrderId}", orderId);

                var result = await _commentRepo.GetCommentById(orderId);

                var comment = result.Select(c => new CommentDTO
                {
                    Id = c.Id,
                    UserName = user.UserName,
                    ToOrderId = c.toOrderId,
                    Content = c.Content,
                    SendDateTime = c.SendDateTime
                });

                _logger.LogInformation(CustomLogEvents.CommentController_Get,
                    "Returned {Count} comments for OrderId {OrderId}", result.Count(), orderId);

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
                    "Error while getting comments for OrderId {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPost("create-comment")]
        public async Task<ActionResult> CreateComment([FromBody] CreatedCommentDTO? comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation(CustomLogEvents.CommentController_Post,
                        "Creating comment for OrderId {OrderId} by UserId {UserId}",
                        comment.ToOrderId, comment.FromUserId);

                    var newComment = new Comment
                    {
                        fromUserId = comment.FromUserId,
                        toOrderId = comment.ToOrderId,
                        Content = comment.Content,
                        SendDateTime = DateTime.UtcNow
                    };

                    var result = await _commentRepo.CreateComment(newComment);

                    _logger.LogInformation(CustomLogEvents.CommentController_Post,
                        "Comment {CommentId} created successfully for OrderId {OrderId}",
                        result.Id, comment.ToOrderId);

                    return StatusCode(StatusCodes.Status201Created,
                        $"Comment '{result.Id}' has been created");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.CommentController_Post,
                        "Invalid model state while creating comment");

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
                    "Error while creating comment");

                var exceptionDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPut("update-comment/{CommentId}")]
        public async Task<ActionResult<Comment>> UpdateComment(int CommentId, [FromBody] UpdatedCommentDTO? comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation(CustomLogEvents.CommentController_Put,
                        "Updating comment {CommentId}", CommentId);

                    var newComment = new Comment
                    {
                        Id = CommentId,
                        Content = comment.Content,
                        SendDateTime = DateTime.UtcNow
                    };

                    var updated = await _commentRepo.UpdateComment(newComment);

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
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpDelete("delete-comment/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.CommentController_Delete,
                    "Deleting comment {CommentId}", id);

                await _commentRepo.DeleteComment(id);

                _logger.LogInformation(CustomLogEvents.CommentController_Delete,
                    "Comment {CommentId} deleted successfully", id);

                return StatusCode(StatusCodes.Status200OK, $"Comment '{id}' has been deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.CommentController_Delete, ex,
                    "Error deleting comment {CommentId}", id);

                var exceptionDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }
    }
}
