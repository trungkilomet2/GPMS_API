using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetEmployees([FromQuery] RequestDTO<EmployeeDTO> input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Getting all employees - PageIndex: {PageIndex}, PageSize: {PageSize}",
                    input.PageIndex, input.PageSize);

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
                        (u.Email != null && u.Email.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase))
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
                        Role = string.Join(", ", u.Roles.Select(r => r.Name)),
                        WorkerRole = string.Join(", ", u.WorkerSkills.Select(w => w.Name)),
                        Status = u.Status?.Name ?? "Unknown"
                    })
                    .ToList();

                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Returned {Count} employees", data.Count);

                return Ok(new RestDTO<IEnumerable<EmployeeDTO>>
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
                    "Error while getting employees");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }

        [HttpGet("get-employee-by-id/{userId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetEmployeeById(int userId)
        {
            if(userId <= 0)
                {
                _logger.LogWarning(CustomLogEvents.WorkerController_Get,
                    "Invalid employee Id {EmployeeId}", userId);
                return StatusCode(StatusCodes.Status400BadRequest,$"Invalid employee Id '{userId}'");
            }
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Getting employee with Id {EmployeeId}", userId);

                var result = await _workerRepo.GetEmployeeById(userId);

                if (result == null)
                {
                    _logger.LogWarning(CustomLogEvents.WorkerController_Get,
                        "Employee with Id {EmployeeId} not found", userId);

                    return StatusCode(StatusCodes.Status404NotFound,
                        $"Employee with Id '{userId}' not found");
                }

                var employee = new EmployeeDTO
                {
                    Id = result.Id,
                    UserName = result.UserName,
                    FullName = result.FullName,
                    AvatarUrl = result.AvartarUrl,
                    PhoneNumber = result.PhoneNumber,
                    Email = result.Email,
                    Role = string.Join(", ", result.Roles.Select(r => r.Name)),
                    WorkerRole = string.Join(", ", result.WorkerSkills.Select(w => w.Name)),
                    Status = result.Status?.Name ?? "Unknown"
                };

                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Returned employee with Id {EmployeeId}", userId);

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
                    "Error while getting employee with Id {EmployeeId}", userId);

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // api/worker
        [HttpPost("create-employee")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> CreateWorker([FromBody] CreateEmployeeDTO? input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Post,
                    "Creating new worker {UserName}", input?.UserName);

                if (ModelState.IsValid)
                {
                    var passwordHasher = new PasswordHasher<User>();
                    var newUser = new User
                    {
                        UserName = input.UserName,
                        PasswordHash = passwordHasher.HashPassword(null, input.Password),
                        FullName = input.FullName,
                        StatusId = 1,
                        Roles = input.RoleIds?.Select(r => new Role
                        {
                            Id = r
                        }).ToList()
                    };

                    var result = await _workerRepo.CreateEmployee(newUser);

                    _logger.LogInformation(CustomLogEvents.WorkerController_Post,
                        "Worker {UserId} created successfully", result.Id);

                    return StatusCode(StatusCodes.Status201Created,
                        $"Worker '{result.Id}' has been created");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.WorkerController_Post,
                        "Invalid model state while creating Worker {UserName}", input?.UserName);

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
                    "Error occurred while creating employee {UserName}", input?.UserName);

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
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> UpdateWorker(int userId, [FromBody] UpdateEmployeeDTO input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Put,
                    "Updating employee {UserId}", userId);

                if (userId <= 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        $"Invalid employee Id '{userId}'");
                }

                if (ModelState.IsValid)
                {
                    var updatedUser = new User
                    {
                        Id = userId,
                        FullName = input.FullName,
                        StatusId = input.StatusId,
                        Roles = input.RoleIds?.Select(r => new Role
                        {
                            Id = r
                        }).ToList()
                    };

                    var result = await _workerRepo.UpdateEmployee(userId, updatedUser);

                    _logger.LogInformation(CustomLogEvents.WorkerController_Put,
                        "Employee {UserId} updated successfully", userId);

                    return Ok($"Employee '{userId}' has been updated");
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
                    "Error occurred while updating employee {UserId}", userId);

                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
