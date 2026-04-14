using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Security.Claims;

namespace GMPS.API.Middlewares
{
    public class DynamicPermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DynamicPermissionMiddleware> _logger;

        public DynamicPermissionMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, ILogger<DynamicPermissionMiddleware> logger)
        {
            _next = next;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint?.Metadata.GetMetadata<IAuthorizeData>() == null)
            {
                await _next(context);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (actionDescriptor == null)
            {
                await _next(context);
                return;
            }

            var controllerName = actionDescriptor.ControllerName;
            var actionName = actionDescriptor.ActionName;
            var httpMethod = context.Request.Method.ToUpper();

            _logger.LogInformation("[DynPerm] {Method} {Controller}/{Action}", httpMethod, controllerName, actionName);

            using var scope = _scopeFactory.CreateScope();
            var permissionRepo = scope.ServiceProvider.GetRequiredService<IPermissionRepositories>();

            var permission = await permissionRepo.GetByEndpoint(controllerName, httpMethod, actionName);

            _logger.LogInformation("[DynPerm] DB result: {Result}", permission == null ? "null" : $"RoleIds='{permission.RoleIds}'");

            if (permission == null || string.IsNullOrWhiteSpace(permission.RoleIds))
            {
                await _next(context);
                return;
            }

            var allowedRoleIds = permission.RoleIds
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .ToHashSet();

            if (allowedRoleIds.Count == 0)
            {
                await _next(context);
                return;
            }

            var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var userRoleIds = userRoles
                .Select(c => GetRoleId(c))
                .Where(id => id > 0)
                .ToHashSet();

            _logger.LogInformation("[DynPerm] UserRoles={UserRoles} | AllowedIds={AllowedIds} | UserIds={UserIds}",
                string.Join(",", userRoles),
                string.Join(",", allowedRoleIds),
                string.Join(",", userRoleIds));

            if (userRoleIds.Overlaps(allowedRoleIds))
            {
                await _next(context);
                return;
            }

            _logger.LogWarning("[DynPerm] BLOCKED {Method} {Controller}/{Action} | User roles: {Roles}",
                httpMethod, controllerName, actionName, string.Join(",", userRoles));

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                Status = 403,
                Message = "Bạn không có quyền thực hiện thao tác này."
            });
        }

        private static int GetRoleId(string roleName)
        {
            return roleName switch
            {
                Roles_Constants.Admin => RoleId_Constants.Admin,
                Roles_Constants.Owner => RoleId_Constants.Owner,
                Roles_Constants.PM => RoleId_Constants.PM,
                Roles_Constants.Worker => RoleId_Constants.Worker,
                Roles_Constants.Customer => RoleId_Constants.Customer,
                _ => -1
            };
        }
    }
}
