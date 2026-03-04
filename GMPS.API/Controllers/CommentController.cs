using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepositories _commentRepo;

        private readonly IConfiguration _configuration;

        public CommentController(ICommentRepositories commentInterface, IConfiguration configuration)
        {
            _commentRepo = commentInterface ?? throw new ArgumentNullException(nameof(commentInterface));
            _configuration = configuration;
        }

        [HttpGet("{orderId}")]
        public async Task<RestDTO<IEnumerable<Comment>>> GetCommentByOrderId(int orderId)
        {
            var result = await _commentRepo.GetCommentById(orderId);
            return new RestDTO<IEnumerable<Comment>>
            {
                Data = result,
                RecordCount = result.Count(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO (Url.Action(null,"Comment", null, Request.Scheme)!,"self","GET")
                }
            };
        }

        [HttpPost]
        public async Task<ActionResult> CreateComment(CreatedCommentDTO? comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newComment = new Comment
                    {
                        fromUserId = comment.FromUserId,
                        toOrderId = comment.ToOrderId,
                        Content = comment.Content,
                        SendDateTime = DateTime.UtcNow
                    };
                    var result = await _commentRepo.Create(newComment);
                    return StatusCode(StatusCodes.Status201Created, $"Comment '{result.Id}' has been created");
                }
                else
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, string.Join("",$"{errorDetails.Errors}") );
                }
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails.Detail);
            }

        }

        [HttpPut("{CommentId}")]
        public async Task<ActionResult<Comment>> UpdateComment(int CommentId, [FromBody] UpdatedCommentDTO? comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newComment = new Comment
                    {
                        Id = CommentId,
                        Content = comment.Content,
                        SendDateTime = DateTime.UtcNow
                    };
                    var updated = await _commentRepo.Update(newComment);
                    return StatusCode(StatusCodes.Status200OK, updated);

                }
                else                 
                {
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
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _commentRepo.Delete(id);
                    return StatusCode(StatusCodes.Status200OK, $"Comment '{id}' has been deleted");
                }
                else
                {
                    var errorDetails = new ValidationProblemDetails(ModelState);
                    errorDetails.Status = StatusCodes.Status400BadRequest;
                    errorDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails.Errors);
                }
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
