using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionRepositories _permissionRepo;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(IPermissionRepositories permissionRepo, ILogger<PermissionController> logger)
        {
            _permissionRepo = permissionRepo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.PermissionController_Get, "Getting all permissions");

                var roleMap = await _permissionRepo.GetRoleMap();
                var permissions = await _permissionRepo.GetAll();
                var result = permissions.Select(p => new
                {
                    controller = p.Controller,
                    action = p.Action,
                    roleIds = p.RoleIds,
                    roles = string.IsNullOrEmpty(p.RoleIds)
                        ? new List<string>()
                        : p.RoleIds.Split(',').Select(id => roleMap.TryGetValue(id, out var name) ? name : id).ToList()
                });

                _logger.LogInformation(CustomLogEvents.PermissionController_Get, "Returned {Count} permissions", result.Count());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.PermissionController_Get, ex, "Error occurred while getting permissions");

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
