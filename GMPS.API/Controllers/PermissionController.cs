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
                    id = p.Id,
                    controller = p.Controller,
                    method = p.Method,
                    action = p.Action,
                    roleIds = p.RoleIds,
                    roles = string.IsNullOrEmpty(p.RoleIds)
                        ? new List<string>()
                        : p.RoleIds.Split(',').Select(rid => roleMap.TryGetValue(rid, out var name) ? name : rid).ToList()
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionDTO input)
        {
            try
            {
                if (id <= 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ValidationProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Errors = { { "id", new[] { "Id must be a positive integer." } } }
                    });
                }

                _logger.LogInformation(CustomLogEvents.PermissionController_Put, "Updating permission {Id}", id);

                var updated = await _permissionRepo.UpdateRoleAuthorize(id, input.RoleAuthorize);
                if (!updated)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ProblemDetails
                    {
                        Detail = $"Permission with id '{id}' not found.",
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    });
                }

                _logger.LogInformation(CustomLogEvents.PermissionController_Put, "Updated permission {Id}", id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.PermissionController_Put, ex, "Error occurred while updating permission {Id}", id);

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
