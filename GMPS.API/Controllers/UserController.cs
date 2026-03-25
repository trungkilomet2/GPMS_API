using AutoMapper;
using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Enums;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using GPMS.INFRASTRUCTURE.DataContext;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepositories _userRepo;
        private readonly IEmailRepositories _emailRepo;
        private readonly IConfiguration _configuration;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepositories userInterface, 
            IConfiguration configuration, ILogger<UserController> logger, 
            ICloudinaryService cloudinaryService, IMemoryCache memoryCache, IEmailRepositories emailRepo)
        {
            _userRepo = userInterface ?? throw new ArgumentNullException(nameof(userInterface));
            _configuration = configuration;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
            _memoryCache = memoryCache;
            _emailRepo = emailRepo ?? throw new ArgumentNullException(nameof(emailRepo));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Owner")]
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


        [HttpGet("user-detail/{userId}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<RestDTO<UserDetailDTO>>> GetUserDetail(int userId)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Get,
                    "Getting user detail for UserId {UserId}", userId);

                if (ModelState.IsValid)
                {
                    var user = await _userRepo.GetUserById(userId);

                    if (user == null)
                    {
                        _logger.LogWarning(CustomLogEvents.UserController_Get,
                            "User not found for UserId {UserId}", userId);

                        return NotFound(new ProblemDetails
                        {
                            Detail = $"User with ID {userId} not found.",
                            Status = StatusCodes.Status404NotFound,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                        });
                    }

                    var userDetail = new UserDetailDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        AvatarUrl = user.AvartarUrl,
                        Location = user.Location,
                        Status = user.Status?.Name ?? "Unknown",
                        Role = string.Join(", ", user.Roles.Select(r => r.Name)),
                        WorkerRole = string.Join(", ", user.WorkerSkills.Select(w => w.Name))
                    };

                    _logger.LogInformation(CustomLogEvents.UserController_Get,
                        "User detail retrieved successfully for UserId {UserId}", userId);

                    return StatusCode(StatusCodes.Status200OK, new RestDTO<UserDetailDTO>
                    {
                        Data = userDetail,
                        Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action("GetUserDetail", "User", new { userId }, Request.Scheme)!,
                        "self",
                        "GET"
                    )
                }
                    });
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Get,
                        "Invalid model state when getting user detail for UserId {UserId}", userId);

                    var details = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };

                    return BadRequest(details);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Get, ex,
                    "Error occurred while retrieving user details for UserId {UserId}", userId);

                var exceptionDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpGet("admin/user-list")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RestDTO<IEnumerable<UserListDTO>>>> GetUserListForAdmin([FromQuery] RequestDTO<UserListDTO> input)
        {
            _logger.LogInformation(CustomLogEvents.UserController_Get, "Admin requesting user list");
            try
            {
                var users = await _userRepo.GetAllUser();
                var data = users.Select(u => new UserListDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    AvartarUrl = u.AvartarUrl,
                    Location = u.Location,
                    Email = u.Email,
                    StatusId = u.StatusId
                }).ToList();

                _logger.LogInformation(CustomLogEvents.UserController_Get, "Retrieved {Count} users successfully", data.Count);

                return Ok(new RestDTO<IEnumerable<UserListDTO>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = data.Count,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "User", new { input.PageIndex, input.PageSize }, Request.Scheme)!, "self", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Get, ex, "Error occurred while retrieving user list");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = "An error occurred while loading the user list."
                });
            }
        }

        [HttpPost("admin/create-user")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RestDTO<UserListDTO>>> CreateUser([FromBody] CreateUserDTO input)
        {
            _logger.LogInformation(CustomLogEvents.UserController_Post, "Admin creating new user with UserName: {UserName}", input.UserName);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Post, "Invalid model state when creating user");
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    return BadRequest(errorDetails);
                }

                var newUser = new User
                {
                    UserName = input.UserName,
                    PasswordHash = input.Password,
                    FullName = input.FullName,
                    StatusId = 1
                };

                var createdUser = await _userRepo.CreateNewUser(newUser, input.RoleIds);

                _logger.LogInformation(CustomLogEvents.UserController_Post, "User created successfully with UserName: {UserName}", createdUser.UserName);

                var result = new UserListDTO
                {
                    Id = createdUser.Id,
                    UserName = createdUser.UserName,
                    FullName = createdUser.FullName,
                    PhoneNumber = createdUser.PhoneNumber,
                    AvartarUrl = createdUser.AvartarUrl,
                    Location = createdUser.Location,
                    Email = createdUser.Email,
                    StatusId = createdUser.StatusId
                };

                return StatusCode(StatusCodes.Status201Created, new RestDTO<UserListDTO>
                {
                    Data = result,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "User", new { id = createdUser.Id }, Request.Scheme)!, "self", "POST")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Post, ex, "Error occurred while creating user with UserName: {UserName}", input.UserName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = "An error occurred while creating the user."
                });
            }
        }

        [HttpPut("admin/disable/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DisableUser(int id)
        {
            _logger.LogInformation(CustomLogEvents.UserController_Put, "Admin disabling UserId {UserId}", id);
            try
            {
                await _userRepo.DisableAnUser(id);
                _logger.LogInformation(CustomLogEvents.UserController_Put, "UserId {UserId} disabled successfully", id);
                return Ok($"User with ID {id} has been disabled.");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.UserController_Put, ex.Message);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Detail = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(CustomLogEvents.UserController_Put, ex.Message);
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Put, ex, "Error occurred while disabling UserId {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = "An error occurred while disabling the user."
                });
            }
        }

        [HttpPut("admin/assign-roles/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignRoles(int id, [FromBody] AssignRoleDTO input)
        {
            _logger.LogInformation(CustomLogEvents.UserController_Put, "Admin assigning roles to UserId {UserId}", id);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Put, "Invalid model state when assigning roles to UserId {UserId}", id);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    return BadRequest(errorDetails);
                }

                await _userRepo.AssignRoles(id, input.RoleIds);

                _logger.LogInformation(CustomLogEvents.UserController_Put, "Roles assigned successfully to UserId {UserId}", id);
                return Ok($"Roles assigned successfully to user with ID {id}.");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.UserController_Put, ex.Message);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Put, ex, "Error occurred while assigning roles to UserId {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = "An error occurred while assigning roles."
                });
            }
        }

        [HttpPut("update-user-for-admin/{userId}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RestDTO<User>>> UpdateUserForAdmin(int userId, [FromForm] UpdatedUserDTO? user)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Put,
                    "Admin updating user for UserId {UserId}", userId);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Put,
                        "Invalid model state when updating user for UserId {UserId}", userId);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };

                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                string? imageUrl = null;

                if (user.AvartarUrl != null)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(
                        user.AvartarUrl,
                        CloudinaryConstrants.Cloudinary_Profile_Image_Folder
                    );

                    imageUrl = uploadResult.Url;
                }

                var updateUser = new User
                {
                    Id = userId,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    AvartarUrl = imageUrl,
                    Location = user.Location,
                    Email = user.Email
                };

                var updatedUser = await _userRepo.UpdateUserForAdmin(userId, updateUser);

                _logger.LogInformation(CustomLogEvents.UserController_Put,
                    "User updated successfully for UserId {UserId}", userId);

                return StatusCode(StatusCodes.Status200OK, new RestDTO<User>
                {
                    Data = updatedUser,
                    Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action("UpdateUserForAdmin", "User", new { userId }, Request.Scheme)!,
                    "self",
                    "PUT"
                )
            }
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.UserController_Put, ex.Message);

                return NotFound(new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Put, ex,
                    "Error occurred while updating user for UserId {UserId}", userId);

                var exceptionDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpGet("view-profile")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [Authorize(Roles = "Admin,Customer,Owner,PM,Team Leader,Worker,KCS")]
        public async Task<ActionResult<RestDTO<ViewProfileDTO>>> ViewProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Get, "Viewing profile for UserId {UserId}", userId);
                if (ModelState.IsValid)
                {
                    var user = await _userRepo.ViewProfile(userId);
                    if (user == null)
                    {
                        _logger.LogWarning(CustomLogEvents.UserController_Get, "User profile not found for UserId {UserId}", userId);
                        return NotFound(new ProblemDetails
                        {
                            Detail = $"User with ID {userId} not found.",
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

                    _logger.LogInformation(CustomLogEvents.UserController_Get, "Profile retrieved successfully for UserId {UserId}", userId);
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
                    _logger.LogWarning(CustomLogEvents.UserController_Get, "Invalid model state when viewing profile for UserId {UserId}", userId);

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
                    "Error occurred while viewing profile for UserId {UserId}", userId);

                var exceptionDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message,
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPut("update-profile")]
        [Authorize(Roles = "Admin,Owner,Team Leader,KCS,Worker,PM,Customer")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RestDTO<User>>> UpdateUser([FromForm] UpdatedUserDTO? user)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Put, "Updating profile for UserId {UserId}", userId);
                if (ModelState.IsValid)
                {
                    var existingUser = await _userRepo.GetUserById(userId);
                    string finalEmail = existingUser.Email;
                    bool isEmailChanged = false;
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        var newEmail = user.Email.Trim().ToLower();
                        var currentEmail = existingUser.Email.Trim().ToLower();

                        if (newEmail != currentEmail)
                        {
                            isEmailChanged = true;

                            var isVerified = _memoryCache.Get<bool?>($"{user.Email}_verified");

                            if (isVerified != true)
                            {
                                _logger.LogWarning(CustomLogEvents.UserController_Put, "Email not verified for UserId {UserId}", userId);
                                var errorDetails = new ValidationProblemDetails(ModelState);
                                errorDetails.Status = StatusCodes.Status400BadRequest;
                                errorDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                                errorDetails.Detail = "Email chưa được xác thực. Vui lòng xác thực email trước khi cập nhật.";
                                return StatusCode(StatusCodes.Status400BadRequest, errorDetails.Detail);
                            }
                            finalEmail = newEmail;
                        }
                        
                    }
                    string? imageUrl = null;
                    if (user.AvartarUrl != null)
                    {
                        var uploadResult = await _cloudinaryService.UploadImageAsync(user.AvartarUrl, CloudinaryConstrants.Cloudinary_Profile_Image_Folder);
                        imageUrl = uploadResult.Url;
                    }
                    var result = new User
                    {
                        Id = userId,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        AvartarUrl = imageUrl,
                        Location = user.Location,
                        Email = user.Email
                    };
                    var updatedUser = await _userRepo.UpdateProfile(userId, result);
                    _memoryCache.Remove($"{user.Email}_verified");
                    _memoryCache.Remove($"{user.Email}_email_otp");
                    _logger.LogInformation(CustomLogEvents.UserController_Put, "Profile updated successfully for UserId {UserId}", userId);

                    return StatusCode(StatusCodes.Status200OK, new RestDTO<User>
                    {
                        Data = updatedUser,
                        Links = new List<LinkDTO>
                            {
                            new LinkDTO(Url.Action(null, "User", new { id = updatedUser.Id }, Request.Scheme)!,"self","PUT")
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
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.UserController_Put, ex.Message);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Put, ex, "Error occurred while updating profile for UserId {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = ex.Message
                });
            }
        }
    }
}
