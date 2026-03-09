using AutoMapper;
using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Enums;
using GPMS.INFRASTRUCTURE.DataContext;
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
    [Authorize(Roles = "Admin,Owner,PM")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepositories _userRepo;

        private readonly IConfiguration _configuration;

        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepositories userInterface, IConfiguration configuration, ILogger<UserController> logger)
        {
            _userRepo = userInterface ?? throw new ArgumentNullException(nameof(userInterface));
            _configuration = configuration;
            _logger = logger;
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

        [HttpPut("update-profile/{userId}")]
        [Authorize(Roles = "Admin,Owner,Team_Leader,KCS,Worker,PM,Customer")]
        public async Task<ActionResult<RestDTO<User>>> UpdateUser(int userId, [FromBody] UpdatedUserDTO? user)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Put, "Updating profile for UserId {UserId}", userId);
                if (ModelState.IsValid)
                {
                    var result = new User
                    {
                        Id = userId,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        AvartarUrl = user.AvartarUrl,
                        Location = user.Location,
                        Email = user.Email
                    };
                    var updatedUser = await _userRepo.UpdateProfile(userId, result);

                    _logger.LogInformation(CustomLogEvents.UserController_Put, "Profile updated successfully for UserId {UserId}", userId);

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
                    _logger.LogWarning(CustomLogEvents.UserController_Put, "Invalid model state when updating profile for UserId {UserId}", userId);
                    var errorDetails = new ValidationProblemDetails(ModelState);
                    errorDetails.Status = StatusCodes.Status400BadRequest;
                    errorDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    return BadRequest(errorDetails.Errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Put, ex, "Error occurred while updating profile for UserId {UserId}", userId);

                var exceptionDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpGet("view-profile/{id}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [Authorize(Roles = "Admin,Customer,Owner,PM,Team_Leader,Worker,KCS")]
        public async Task<ActionResult<RestDTO<ViewProfileDTO>>> ViewProfile(int id)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Get, "Viewing profile for UserId {UserId}", id);
                if (ModelState.IsValid)
                {
                    var user = await _userRepo.ViewProfile(id);
                    if (user == null)
                    {
                        _logger.LogWarning(CustomLogEvents.UserController_Get, "User profile not found for UserId {UserId}", id);
                        return NotFound(new ProblemDetails
                        {
                            Detail = $"User with ID {id} not found.",
                            Status = StatusCodes.Status404NotFound,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                        });
                    }
                    var profile = new ViewProfileDTO
                    {
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        AvartarUrl = user.AvartarUrl,
                        Location = user.Location,
                        Email = user.Email
                    };

                    _logger.LogInformation(CustomLogEvents.UserController_Get, "Profile retrieved successfully for UserId {UserId}", id);
                    return StatusCode(StatusCodes.Status200OK, new RestDTO<ViewProfileDTO>
                    {
                        Data = profile,
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(
                                Url.Action("ViewProfile", "User", null, Request.Scheme)!,
                                "self",
                                "GET"
                            )
                        }
                    });
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Get, "Invalid model state when viewing profile for UserId {UserId}", id);

                    var details = new ValidationProblemDetails(ModelState);
                    details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Get, ex,
                    "Error occurred while viewing profile for UserId {UserId}", id);

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
