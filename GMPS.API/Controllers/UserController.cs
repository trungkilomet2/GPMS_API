using GMPS.API.DTOs;
using GPMS.APPLICATION.Abstractions;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        public readonly IUserInterface _userInterface;
        public UserController(IUserInterface userInterface)
        {
            _userInterface = userInterface ?? throw new ArgumentNullException(nameof(userInterface));
        }

        [HttpGet]
        public async Task<RestDTO<IEnumerable<User>>> GetUser([FromQuery] RequestDTO<User> input)
        {
            var result = await _userInterface.GetUser();
            return new RestDTO<IEnumerable<User>>
            {
                Data = result,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = result.Count(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO (Url.Action(null,"User", new { input.PageIndex, input.PageSize }, Request.Scheme)!,"self","GET")
                }
            };

        }
    }
}
