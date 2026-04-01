using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly IPermissionRepositories _permissionRepo;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IPermissionRepositories permissionRepo, ILogger<RoleController> logger)
        {
            _permissionRepo = permissionRepo ?? throw new ArgumentNullException(nameof(permissionRepo));
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<RestDTO<IEnumerable<RoleDTO>>>> GetAllRoles()
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.RoleController_Get, "Getting all roles");

                var roles = await _permissionRepo.GetAllRoles();
                var data = roles.Select(r => new RoleDTO
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();

                _logger.LogInformation(CustomLogEvents.RoleController_Get, "Returned {Count} roles", data.Count);

                return Ok(new RestDTO<IEnumerable<RoleDTO>>
                {
                    Data = data,
                    RecordCount = data.Count,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "Role", null, Request.Scheme)!, "self", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.RoleController_Get, ex, "Error occurred while getting roles");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }
    }
}
