using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin")]
    //[Authorize(Roles = "Owner")]
    //[Authorize(Roles = "PM")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepositories _userRepo;

        private readonly IConfiguration _configuration;

        public UserController(IUserRepositories userInterface, IConfiguration configuration)
        {
            _userRepo = userInterface ?? throw new ArgumentNullException(nameof(userInterface));
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<RestDTO<IEnumerable<User>>> GetUser([FromQuery] RequestDTO<User> input)
        {
            var result = await _userRepo.GetAllUser();
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

        [HttpPut("{userId}")]
        public async Task<ActionResult<RestDTO<User>>> UpdateUser(int userId, [FromBody] UpdatedUserDTO user)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var result = new User
                    {
                        UserName = user.UserName,
                        PasswordHash = user.PasswordHash,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        AvartarUrl = user.AvartarUrl,
                        Email = user.Email
                    };
                    var updatedUser = await _userRepo.UpdateProfile(userId, result);
                    return StatusCode(StatusCodes.Status200OK, new RestDTO<User>
                    {
                        Data = updatedUser,
                        Links = new List<LinkDTO>
        {
            new LinkDTO(
                Url.Action(null, "User", new { id = updatedUser.Id }, Request.Scheme)!,
                "self",
                "PUT"
            )
        }
                    });
                }
                else
                {
                    var errorDetails = new ValidationProblemDetails(ModelState);
                    errorDetails.Status = StatusCodes.Status400BadRequest;
                    errorDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    return BadRequest(errorDetails);
                }
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status =
                StatusCodes.Status500InternalServerError;
                exceptionDetails.Type =
                "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                return StatusCode(
                StatusCodes.Status500InternalServerError,
                exceptionDetails);
            }
        }
    }
}
