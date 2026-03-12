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
        private readonly IUserRepositories _userRepo;
        private readonly ILogger<AccountController> _logger;

        public WorkerController(IUserRepositories userRepo,ILogger<AccountController> logger)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _logger = logger;
        }

        [HttpGet("get-all-worker")]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.UserController_Get,
                    "Getting all employees");

                var result = await _userRepo.GetAllEmployees();

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
                    Url.Action(null, "User", null, Request.Scheme)!,
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
    }
}
