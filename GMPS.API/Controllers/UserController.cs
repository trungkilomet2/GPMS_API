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
using Microsoft.EntityFrameworkCore;
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
                    "Đang lấy thông tin người dùng với ID {UserId}", userId);

                if (ModelState.IsValid)
                {
                    var user = await _userRepo.GetUserById(userId);

                    if (user == null)
                    {
                        _logger.LogWarning(CustomLogEvents.UserController_Get,
                            "Không tìm thấy người dùng với ID {UserId}", userId);

                        return NotFound(new ProblemDetails
                        {
                            Detail = $"Không tìm thấy người dùng với ID {userId}.",
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
                        Status = user.Status?.Name ?? "Không xác định",
                        Roles = user.Roles.Select(r => r.Name).ToList(),
                        WorkerRole = string.Join(", ", user.WorkerSkills.Select(w => w.Name))
                    };

                    _logger.LogInformation(CustomLogEvents.UserController_Get,
                        "Lấy thông tin người dùng thành công với ID {UserId}", userId);

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
                        "Lỗi dữ liệu đầu vào khi lấy thông tin người dùng với ID {UserId}", userId);

                    var details = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };

                    return BadRequest(details);
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.UserController_Get, ex.Message);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Get, ex,
                    "Lỗi xảy ra khi lấy thông tin người dùng với ID {UserId}", userId);

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
            _logger.LogInformation(CustomLogEvents.UserController_Get, "Admin đang yêu cầu danh sách người dùng");
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
                    StatusId = u.StatusId,
                    StatusName = u.Status?.Name ?? "Không xác định",
                    Roles = u.Roles.Select(r => r.Name).ToList()
                }).ToList();

                _logger.LogInformation(CustomLogEvents.UserController_Get, "Lấy danh sách {Count} người dùng thành công", data.Count);

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
                _logger.LogError(CustomLogEvents.UserController_Get, ex, "Lỗi xảy ra khi lấy danh sách người dùng");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = "Có lỗi xảy ra khi tải danh sách người dùng."
                });
            }
        }

        [HttpPost("admin/create-user")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RestDTO<UserListDTO>>> CreateUser([FromBody] CreateUserDTO input)
        {
            _logger.LogInformation(CustomLogEvents.UserController_Post, "Admin đang tạo người dùng mới với tên tài khoản: {UserName}", input.UserName);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Post, "Lỗi dữ liệu đầu vào khi tạo người dùng");
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
                    StatusId = UserStatus_Constants.Active
                };

                var createdUser = await _userRepo.CreateNewUser(newUser, input.RoleIds);

                _logger.LogInformation(CustomLogEvents.UserController_Post, "Tạo người dùng thành công với tên tài khoản: {UserName}", createdUser.UserName);

                var result = new UserListDTO
                {
                    Id = createdUser.Id,
                    UserName = createdUser.UserName,
                    FullName = createdUser.FullName,
                    PhoneNumber = createdUser.PhoneNumber,
                    AvartarUrl = createdUser.AvartarUrl,
                    Location = createdUser.Location,
                    Email = createdUser.Email,
                    StatusId = createdUser.StatusId,
                    StatusName = "Active",
                    Roles = new List<string>()
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
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(CustomLogEvents.UserController_Post, ex.Message);
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.UserController_Post, ex, "Lỗi xảy ra khi tạo người dùng với tên tài khoản: {UserName}", input.UserName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = "Có lỗi xảy ra khi tạo người dùng."
                });
            }
        }

        [HttpPut("admin/disable/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DisableUser(int id)
        {
            _logger.LogInformation(CustomLogEvents.UserController_Put, "Admin đang thay đổi trạng thái người dùng với ID {UserId}", id);
            try
            {
                var isActive = await _userRepo.DisableAnUser(id);
                var action = isActive ? "kích hoạt lại" : "vô hiệu hóa";
                _logger.LogInformation(CustomLogEvents.UserController_Put, "Người dùng với ID {UserId} đã được {Action} thành công", id, action);
                return Ok(isActive
                    ? $"Người dùng với ID {id} đã được kích hoạt lại."
                    : $"Người dùng với ID {id} đã bị vô hiệu hóa.");
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
                _logger.LogError(CustomLogEvents.UserController_Put, ex, "Lỗi xảy ra khi thay đổi trạng thái người dùng với ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = "Có lỗi xảy ra khi thay đổi trạng thái người dùng."
                });
            }
        }

        [HttpPut("admin/assign-roles/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignRoles(int id, [FromBody] AssignRoleDTO input)
        {
            _logger.LogInformation(CustomLogEvents.UserController_Put, "Admin đang phân quyền cho người dùng với ID {UserId}", id);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Put, "Lỗi dữ liệu đầu vào khi phân quyền cho người dùng với ID {UserId}", id);
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    return BadRequest(errorDetails);
                }

                await _userRepo.AssignRoles(id, input.RoleIds);

                _logger.LogInformation(CustomLogEvents.UserController_Put, "Phân quyền thành công cho người dùng với ID {UserId}", id);
                return Ok($"Phân quyền thành công cho người dùng với ID {id}.");
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
                _logger.LogError(CustomLogEvents.UserController_Put, ex, "Lỗi xảy ra khi phân quyền cho người dùng với ID {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Detail = "Có lỗi xảy ra khi phân quyền."
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
                    "Admin đang cập nhật người dùng với Id là: {UserId}", userId);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Put,
                        "Lỗi model state khi cập nhật người dùng với Id là: {UserId}", userId);

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
                    "Cập nhật người dùng thành công với ID {UserId}", userId);

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
                _logger.LogError(CustomLogEvents.UserController_Put, ex,
                    "Có lỗi xảy ra khi cập nhật người dùng với Id là: {UserId}", userId);

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
        [Authorize(Roles = "Admin,Customer,Owner,PM,Worker")]
        public async Task<ActionResult<RestDTO<ViewProfileDTO>>> ViewProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Get, "đang xem thông tin của người dùng với Id là: {UserId}", userId);
                if (ModelState.IsValid)
                {
                    var user = await _userRepo.ViewProfile(userId);
                    if (user == null)
                    {
                        _logger.LogWarning(CustomLogEvents.UserController_Get, "Không tìm thấy người dùng với Id là: {UserId}", userId);
                        return NotFound(new ProblemDetails
                        {
                            Detail = $"Người dùng với ID {userId} không thấy.",
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

                    _logger.LogInformation(CustomLogEvents.UserController_Get, "Thành công lấy về thông tin người dùng với Id là: {UserId}", userId);
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
                    _logger.LogWarning(CustomLogEvents.UserController_Get, "Lỗi model state khi xem thông tin của người dùng với Id là: {UserId}", userId);

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
                    "Lỗi xảy ra khi xem thông tin của người dùng với Id là: {UserId}", userId);

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
        [Authorize(Roles = "Admin,Owner,Worker,PM,Customer")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RestDTO<User>>> UpdateUser([FromForm] UpdatedUserDTO? user)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Put, "Cập nhật người dùng với Id là: {UserId}", userId);
                if (ModelState.IsValid)
                {
                    var existingUser = await _userRepo.GetUserById(userId);
                    string finalEmail = existingUser.Email;
                    bool isEmailChanged = false;
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        var newEmail = user.Email.Trim().ToLower();
                        var currentEmail = existingUser.Email?.Trim().ToLower();


                        if (string.IsNullOrEmpty(currentEmail) || newEmail != currentEmail)
                        {
                            isEmailChanged = true;

                            var isVerified = _memoryCache.Get<bool?>($"{user.Email}_verified");

                            if (isVerified != true)
                            {
                                _logger.LogWarning(CustomLogEvents.UserController_Put, "Email chưa được xác thực với Id người dùng là: {UserId}", userId);
                                var errorDetails = new ValidationProblemDetails(ModelState)
                                {
                                    Status = StatusCodes.Status400BadRequest,
                                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                                    Detail = "Email chưa được xác thực. Vui lòng xác thực email trước khi cập nhật.",
                                };
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
                        Email = finalEmail,
                        StatusId = existingUser.StatusId
                    };
                    var updatedUser = await _userRepo.UpdateProfile(userId, result);
                    _memoryCache.Remove($"{user.Email}_verified");
                    _memoryCache.Remove($"{user.Email}_email_otp");
                    _logger.LogInformation(CustomLogEvents.UserController_Put, "Cập nhật thành công người dùng với Id là: {UserId}", userId);

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
                    _logger.LogWarning(CustomLogEvents.UserController_Put, "Lỗi model state khi cập nhật người dùng với Id là: {UserId}", userId);
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
                _logger.LogError(CustomLogEvents.UserController_Put, ex, "Lỗi xảy ra khi cập nhật người dùng với Id là: {UserId}", userId);
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
