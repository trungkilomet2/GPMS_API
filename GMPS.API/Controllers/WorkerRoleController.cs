using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerRoleController : ControllerBase
    {
        private readonly IWorkerRoleRepositories _workerroleRepo;
        private readonly ILogger<AccountController> _logger;

        public WorkerRoleController(IWorkerRoleRepositories workerroleRepo, ILogger<AccountController> logger)
        {
            _workerroleRepo = workerroleRepo ?? throw new ArgumentNullException(nameof(workerroleRepo));
            _logger = logger;
        }

        [HttpGet("get-all-worker-roles")]
        [Authorize(Roles = "Admin,Owner,PM")]
        public async Task<IActionResult> GetAllWorkerRoles([FromQuery] RequestDTO<WorkerRole> input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Getting all worker roles");

                var result = await _workerroleRepo.GetAllWorkerRoles();

                if (!result.Any())
                {
                    _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                        "No worker roles found");

                    return StatusCode(StatusCodes.Status404NotFound, "No worker roles found");
                }

                _logger.LogInformation(CustomLogEvents.WorkerController_Get,
                    "Returned {Count} worker roles", result.Count());

                var response = new RestDTO<IEnumerable<WorkerRole>>
                {
                    Data = result,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = result.Count(),
                    Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(null, "WorkerRole", null, Request.Scheme)!,
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
                    "Error while getting worker roles");

                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }
    }
}
