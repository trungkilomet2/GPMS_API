using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
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
        public async Task<IActionResult> GetAllWorkerRoles([FromQuery] RequestDTO<WorkerSkill> input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerRoleController_Get,
                    "Getting all worker skills - PageIndex: {PageIndex}, PageSize: {PageSize}",
                    input.PageIndex, input.PageSize);

                if (!ModelState.IsValid)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest
                    };
                    return BadRequest(errorDetails);
                }

                var result = await _workerroleRepo.GetAllWorkerRoles();
                if(result == null || !result.Any())
                {
                    _logger.LogInformation(CustomLogEvents.WorkerRoleController_Get,
                        "No worker Skills found");
                    return NotFound("Worker Skill not found");
                }
                if (!string.IsNullOrEmpty(input.FilterQuery))
                {
                    result = result.Where(x =>
                        x.Name != null &&
                        x.Name.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase));
                }

                var recordCount = result.Count();
                var totalPages = (int)Math.Ceiling((double)recordCount / input.PageSize);

                if (recordCount > 0 && input.PageIndex >= totalPages)
                {
                    return NotFound(new
                    {
                        message = $"Page {input.PageIndex} not exist",
                        totalPages
                    });
                }

                var data = result
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize)
                    .ToList();

                _logger.LogInformation(CustomLogEvents.WorkerRoleController_Get,
                    "Returned {Count} worker skills", data.Count);

                return Ok(new RestDTO<IEnumerable<WorkerSkill>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(null, "WorkerRole",
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
                    "Error while getting worker skills");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }

        [HttpPost("create-worker-roles")]
        [Authorize(Roles = "Owner,Admin,PM")]
        public async Task<ActionResult> CreateWorkerRole([FromBody] CreateWorkerRoleDTO? input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerController_Post,
                    "Creating new worker role {Name}", input?.Name);

                if (ModelState.IsValid)
                {
                    var newRole = new WorkerSkill
                    {
                        Name = input.Name
                    };

                    var result = await _workerroleRepo.CreateWorkerRole(newRole);

                    _logger.LogInformation(CustomLogEvents.WorkerController_Post,
                        "Worker Role {Id} created successfully", result.Id);

                    return StatusCode(StatusCodes.Status201Created,
                        $"Worker Role '{result.Id}' has been created");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.WorkerController_Post,
                        "Invalid model state while creating Worker Role {Name}", input?.Name);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };

                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.Error_Post, ex,
                    "Error occurred while creating worker role {Name}", input?.Name);

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
    }
}
