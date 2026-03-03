using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{id}")]
        public async Task<RestDTO<IEnumerable<Comment>>> GetCommentByOrderId(int id)
        {
            var result = await _commentRepo.GetCommentById(id);
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
        public async Task<IActionResult> CreateComment(Comment comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _commentRepo.Create(comment);
                    var response = new RestDTO<Comment>
                    {
                        Data = result,
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(Url.Action(null, "Comment", new { id = result.Id }, Request.Scheme)!, "self", "POST")
                        }
                    };
                    return StatusCode(StatusCodes.Status200OK,response);
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

        [HttpPut("{id}")]
        public async Task<ActionResult<Comment>> Update(int id, Comment comment)
        {
            if (id != comment.Id)
                return BadRequest("Id mismatch");

            var updated = await _commentRepo.Update(comment);

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _commentRepo.Delete(id);
            return NoContent();
        }
    }
}
