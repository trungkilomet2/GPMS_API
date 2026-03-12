using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using Microsoft.AspNetCore.Http;
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

        [HttpGet("get-all-worker")]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Get,
                    "Getting all employees");

                var result = await _workerRepo.GetAllEmployees();

                if (!result.Any())
                {
                    _logger.LogInformation(CustomLogEvents.UserController_Get,
                        "No employees found");

                    return StatusCode(StatusCodes.Status404NotFound, "No employees found");
                }

                var employees = result.Select(u => new EmployeeDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    Role = string.Join(", ", u.Roles.Select(r => r.Name)),
                    WorkerRole = string.Join(", ", u.WorkerRoles.Select(w => w.Name)),
                    Status = u.Status?.Name ?? "Unknown"
                });

                _logger.LogInformation(CustomLogEvents.UserController_Get,
                    "Returned {Count} employees", result.Count());

                var response = new RestDTO<IEnumerable<EmployeeDTO>>
                {
                    Data = employees,
                    RecordCount = result.Count(),
                    Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(null, "Worker", null, Request.Scheme)!,
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
                    "Error while getting employees");

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpGet("get-worker-by-id/{userId}")]
        public async Task<IActionResult> GetEmployeeById(int userId)
        {
            if(userId <= 0)
                {
                _logger.LogWarning(CustomLogEvents.UserController_Get,
                    "Invalid employee Id {EmployeeId}", userId);
                return StatusCode(StatusCodes.Status400BadRequest,$"Invalid employee Id '{userId}'");
            }
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Get,
                    "Getting employee with Id {EmployeeId}", userId);

                var result = await _workerRepo.GetEmployeeById(userId);

                if (result == null)
                {
                    _logger.LogWarning(CustomLogEvents.UserController_Get,
                        "Employee with Id {EmployeeId} not found", userId);

                    return StatusCode(StatusCodes.Status404NotFound,
                        $"Employee with Id '{userId}' not found");
                }

                var employee = new EmployeeDTO
                {
                    Id = result.Id,
                    UserName = result.UserName,
                    FullName = result.FullName,
                    PhoneNumber = result.PhoneNumber,
                    Email = result.Email,
                    Role = string.Join(", ", result.Roles.Select(r => r.Name)),
                    WorkerRole = string.Join(", ", result.WorkerRoles.Select(w => w.Name)),
                    Status = result.Status?.Name ?? "Unknown"
                };

                _logger.LogInformation(CustomLogEvents.UserController_Get,
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
    }
}
