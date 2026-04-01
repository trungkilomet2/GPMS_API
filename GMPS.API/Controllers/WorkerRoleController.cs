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
        public async Task<ActionResult<RestDTO<IEnumerable<WorkerSkill>>>> GetAllWorkerRoles([FromQuery] RequestDTO<WorkerSkill> input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.WorkerRoleController_Get,
                    "Lấy về kỹ năng của nhân viên - PageIndex: {PageIndex}, PageSize: {PageSize}",
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
                        "Không tìm thấy kỹ năng của nhân viên");
                    return NotFound("Không tìm thấy kỹ năng của nhân viên");
                }
                if (!string.IsNullOrEmpty(input.FilterQuery))
                {
                    result = result.Where(x =>
                        x.Name != null &&
                        x.Name.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase));
                }

                var data = result
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize)
                    .ToList();

                _logger.LogInformation(CustomLogEvents.WorkerRoleController_Get,
                    "Trả về {Count} kỹ năng của nhân viên", data.Count);

                return StatusCode(StatusCodes.Status200OK,new RestDTO<IEnumerable<WorkerSkill>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = data.Count(),
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
                    "Lỗi khi lấy về kỹ năng của nhân viên");

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
                    "Tạo kỹ năng mới: {Name}", input?.Name);

                if (ModelState.IsValid)
                {
                    var newRole = new WorkerSkill
                    {
                        Name = input.Name
                    };

                    var result = await _workerroleRepo.CreateWorkerRole(newRole);

                    _logger.LogInformation(CustomLogEvents.WorkerController_Post,
                        "Kỹ năng {Id} được tạo thành công", result.Id);

                    return StatusCode(StatusCodes.Status201Created,
                        $"Kỹ năng '{result.Id}' được tạo thành công");
                }
                else
                {
                    _logger.LogWarning(CustomLogEvents.WorkerController_Post,
                        "Lỗi model state khi tạo kỹ năng: {Name}", input?.Name);

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
                    "Lỗi khi tạo kỹ năng: {Name}", input?.Name);

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
