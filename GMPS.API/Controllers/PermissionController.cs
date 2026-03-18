using GPMS.APPLICATION.Repositories;
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

        public PermissionController(IPermissionRepositories permissionRepo)
        {
            _permissionRepo = permissionRepo;
        }

        [HttpGet]
        public IActionResult GetPermissions()
        {
            var roleMap = _permissionRepo.GetRoleMap();
            var result = _permissionRepo.GetAll().Select(p => new
            {
                controller = p.Controller,
                action = p.Action,
                roleIds = p.RoleIds,
                roles = string.IsNullOrEmpty(p.RoleIds)
                    ? new List<string>()
                    : p.RoleIds.Split(',').Select(id => roleMap.TryGetValue(id, out var name) ? name : id).ToList()
            });

            return Ok(result);
        }
    }
}
