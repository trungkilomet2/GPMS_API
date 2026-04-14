using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        private readonly IWorkerRepositories _workerRepo;
        private readonly ILogger<AccountController> _logger;

        public WorkerController(IWorkerRepositories workerRepo,ILogger<AccountController> logger)
        {
            _workerRepo = workerRepo ?? throw new ArgumentNullException(nameof(workerRepo));
            _logger = logger;
        }

        [HttpGet("get-all-employees")]
        [Authorize]
        public async Task<ActionResult<RestDTO<IEnumerable<EmployeeDTO>>>> GetEmployees([FromQuery] RequestDTO<EmployeeDTO> input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Getting all employees - PageIndex: {PageIndex}, PageSize: {PageSize}, SortColumn: {SortColumn}, SortOrder: {SortOrder}, FilterQuery: {FilterQuery} ",
                    input.PageIndex, input.PageSize, input.SortColumn,input.SortOrder, input.FilterQuery);

                if (!ModelState.IsValid)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest
                    };
                    return BadRequest(errorDetails);
                }

                var result = await _workerRepo.GetAllEmployees();

                if (!string.IsNullOrEmpty(input.FilterQuery?.Trim()))
                {
                    result = result.Where(u =>
                        (u.FullName != null && u.FullName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.UserName != null && u.UserName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.Email != null && u.Email.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        u.Roles != null &&u.Roles.Any(r => r.Name.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        u.WorkerSkills != null &&u.WorkerSkills.Any(r => r.Name.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.Status != null && u.Status.Name.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase))
                    );
                }

                var recordCount = result.Count();
                var data = result
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize)
                    .Select(u => new EmployeeDTO
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Email = u.Email,
                        ManagerId = u.ManagerId,
                        Role = string.Join(", ", u.Roles.Select(r => r.Name)),
                        WorkerRole = string.Join(", ", u.WorkerSkills.Select(w => w.Name)),
                        Status = u.Status?.Name ?? "Unknown"
                    })
                    .ToList();

                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Trả về {Count} nhân viên", data.Count);
                return StatusCode(StatusCodes.Status200OK,new RestDTO<IEnumerable<EmployeeDTO>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(null, "Worker",
                        new { input.PageIndex, input.PageSize },
                        Request.Scheme)!,
                    "self",
                    "GET"
                )
            }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.Error_Get, ex,
                    "Lỗi khi lấy về nhân viên");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }

        [HttpGet("get-all-employees-by-pm-id")]
        [Authorize]
        public async Task<ActionResult<RestDTO<IEnumerable<EmployeeDTO>>>> GetEmployeesbyPMId([FromQuery] RequestDTO<EmployeeDTO> input)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Getting all employees by PM id - PageIndex: {PageIndex}, PageSize: {PageSize}, SortColumn: {SortColumn}, SortOrder: {SortOrder}, FilterQuery: {FilterQuery} ",
                    input.PageIndex, input.PageSize, input.SortColumn, input.SortOrder, input.FilterQuery);

                if (!ModelState.IsValid)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest
                    };
                    return BadRequest(errorDetails);
                }

                var result = await _workerRepo.GetAllEmployeesByPMId(userId);
                if (result == null)
                {
                    _logger.LogInformation("Không tìm thấy nhân viên nào thuộc quyền quản lý của PM: {userId}", userId);
                    return NotFound("Không tìm thấy nhân viên nào thuộc quyền quản lý của PM này");
                }
                    
                if (!string.IsNullOrEmpty(input.FilterQuery?.Trim()))
                {
                    result = result.Where(u =>
                        (u.FullName != null && u.FullName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.UserName != null && u.UserName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.Email != null && u.Email.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        u.Roles != null && u.Roles.Any(r => r.Name.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        u.WorkerSkills != null && u.WorkerSkills.Any(r => r.Name.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.Status != null && u.Status.Name.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase))
                    );
                }

                var recordCount = result.Count();
                var data = result
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize)
                    .Select(u => new EmployeeDTO
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Email = u.Email,
                        ManagerId = u.ManagerId,
                        Role = string.Join(", ", u.Roles.Select(r => r.Name)),
                        WorkerRole = string.Join(", ", u.WorkerSkills.Select(w => w.Name)),
                        Status = u.Status?.Name ?? "Unknown"
                    })
                    .ToList();

                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "trả về {Count} nhân viên", data.Count);
                return StatusCode(StatusCodes.Status200OK,new RestDTO<IEnumerable<EmployeeDTO>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(null, "Worker",
                        new { input.PageIndex, input.PageSize },
                        Request.Scheme)!,
                    "self",
                    "GET"
                )
            }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.Error_Get, ex,
                    "Lỗi khi lấy về nhân viên");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }

        [HttpGet("get-employee-by-id/{userId}")]
        [Authorize]
        public async Task<ActionResult<RestDTO<EmployeeDTO>>> GetEmployeeById(int userId)
        {
            if(userId <= 0)
                {
                _logger.LogWarning(CustomLogEvents.WorkerController_Get,
                    "Id không hợp lệ {EmployeeId}", userId);
                return StatusCode(StatusCodes.Status400BadRequest,$"Id không hợp lệ '{userId}'");
            }
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Lấy về nhân viên với Id: {EmployeeId}", userId);

                var result = await _workerRepo.GetEmployeeById(userId);

                if (result == null)
                {
                    _logger.LogWarning(CustomLogEvents.WorkerController_Get,
                        "Nhân viên với Id {EmployeeId} không tìm thấy", userId);

                    return StatusCode(StatusCodes.Status404NotFound,
                        $"Nhân viên với Id '{userId}' không tìm thấy");
                }

                var employee = new EmployeeDTO
                {
                    Id = result.Id,
                    UserName = result.UserName,
                    FullName = result.FullName,
                    AvatarUrl = result.AvartarUrl,
                    PhoneNumber = result.PhoneNumber,
                    Email = result.Email,
                    ManagerId = result.ManagerId,
                    Role = string.Join(", ", result.Roles.Select(r => r.Name)),
                    WorkerRole = string.Join(", ", result.WorkerSkills.Select(w => w.Name)),
                    Status = result.Status?.Name ?? "Unknown"
                };

                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Trả về nhân viên với Id: {EmployeeId}", userId);

                var response = new RestDTO<EmployeeDTO>
                {
                    Data = employee,
                    RecordCount = 1,
                    Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(null, "Worker", new { userId }, Request.Scheme)!,
                    "self",
                    "GET"
                )
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
                    "Lỗi khi lấy về nhân viên với Id: {EmployeeId}", userId);

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // api/worker
        [HttpPost("create-employee")]
        [Authorize]
        public async Task<ActionResult> CreateWorker([FromBody] CreateEmployeeDTO? input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Post,
                    "Tạo nhân viên với tên tài khoản: {UserName}", input?.UserName);

                if (ModelState.IsValid)
                {

                    if (input.RoleIds == null || !input.RoleIds.Any())
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, "Phải chọn ít nhất 1 role");
                    }

                    int? managerId = input.ManagerId;

                    if (input.RoleIds.Contains(RoleId_Constants.PM))
                    {
                        managerId = 1;
                    }

                    if (input.RoleIds.Contains(RoleId_Constants.Worker))
                    {
                        if (input.ManagerId == null)
                        {
                            return StatusCode(StatusCodes.Status400BadRequest,
                                "Worker bắt buộc phải chọn PM quản lý");
                        }

                        managerId = input.ManagerId;
                    }
                    var passwordHasher = new PasswordHasher<User>();
                    var newUser = new User
                    {
                        UserName = input.UserName,
                        PasswordHash = passwordHasher.HashPassword(null, input.Password),
                        FullName = input.FullName,
                        ManagerId = input.ManagerId,
                        StatusId = UserStatus_Constants.Active,
                        Roles = input.RoleIds?.Select(r => new Role
                        {
                            Id = r
                        }).ToList()
                    };


                    var result = await _workerRepo.CreateEmployee(newUser);

                    _logger.LogInformation(CustomLogEvents.WorkerController_Post,
                        "Nhân viên có Id: {UserId} được tạo thành công", result.Id);

                    return StatusCode(StatusCodes.Status201Created,
                        $"Nhân viên có Id: '{result.Id}' được tạo thành công");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.WorkerController_Post,
                        "Lỗi model state khi tạo nhân viên: {UserName}", input?.UserName);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };

                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.WorkerController_Post, ex.Message);
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.Error_Post, ex,
                    "Lỗi khi tạo nhân viên: {UserName}", input?.UserName);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

                return StatusCode(StatusCodes.Status500InternalServerError,
                    exceptionDetails.Detail);
            }
        }

        [HttpPut("update-employee/{userId}")]
        [Authorize]
        public async Task<ActionResult> UpdateWorker(int userId, [FromBody] UpdateEmployeeDTO input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Put,
                    "Cập nhật nhân viên có Id: {UserId}", userId);

                if (userId <= 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        $"Id không hợp lệ '{userId}'");
                }

                if (ModelState.IsValid)
                {
                    var updatedUser = new User
                    {
                        Id = userId,
                        FullName = input.FullName,
                        ManagerId = input.ManagerId,
                        Roles = input.RoleIds?.Select(r => new Role
                        {
                            Id = r
                        }).ToList()
                    };

                    var result = await _workerRepo.UpdateEmployee(userId, updatedUser);

                    _logger.LogInformation(CustomLogEvents.WorkerController_Put,
                        "Nhân viên có Id: {UserId} cập nhật thành công", userId);

                    return Ok($"Nhân viên có Id: '{userId}' cập nhật thành công");
                }
                else
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest
                    };

                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.Error_Post, ex,
                    "Lỗi khi cập nhật nhân viên có Id: {UserId}", userId);

                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("assign-worker-skill/{userId}")]
        [Authorize]
        public async Task<ActionResult> AssignWorkerSkill(int userId, [FromBody] AssignWorkerRoleDTO input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Put,
                    "Thêm kỹ năng cho nhân viên có Id: {UserId}", userId);

                if (userId <= 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        $"Id không hợp lệ '{userId}'");
                }

                if (ModelState.IsValid)
                {
                    var updatedUser = new User
                    {
                        Id = userId,
                        WorkerSkills = input.SkillIds?.Select(r => new WorkerSkill
                        {
                            Id = r
                        }).ToList()
                    };

                    var result = await _workerRepo.AssignWorkerSkill(userId, updatedUser);

                    _logger.LogInformation(CustomLogEvents.WorkerController_Put,
                        "Nhân viên có Id: {UserId} được thêm kỹ năng thành công", userId);

                    return Ok($"Nhân viên có Id: '{userId}' được thêm kỹ năng thành công");
                }
                else
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                    };

                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.Error_Post, ex,
                    "Lỗi khi cập nhật nhân viên có Id: {UserId}", userId);

                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
