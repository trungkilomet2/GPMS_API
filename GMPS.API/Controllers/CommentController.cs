using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Http;
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
    }
}
