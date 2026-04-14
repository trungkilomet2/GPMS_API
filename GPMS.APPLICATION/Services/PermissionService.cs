using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class PermissionService : IPermissionRepositories
    {
        private readonly IBasePermissionRepositories _permissionBaseRepo;

        public PermissionService(IBasePermissionRepositories permissionBaseRepo)
        {
            _permissionBaseRepo = permissionBaseRepo ?? throw new ArgumentNullException(nameof(permissionBaseRepo));
        }

        public async Task<IEnumerable<PermissionEntry>> GetAll()
        {
            return await _permissionBaseRepo.GetAll();
        }

        public async Task<PermissionEntry?> GetById(int id)
        {
            return await _permissionBaseRepo.GetById(id);
        }

        public async Task<Dictionary<string, string>> GetRoleMap()
        {
            return await _permissionBaseRepo.GetRoleMap();
        }

        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            return await _permissionBaseRepo.GetAllRoles();
        }

        public async Task<bool> UpdateRoleAuthorize(int id, string? roleAuthorize)
        {
            return await _permissionBaseRepo.UpdateRoleAuthorize(id, roleAuthorize);
        }

        public async Task<PermissionEntry?> GetByEndpoint(string controller, string method, string action)
        {
            return await _permissionBaseRepo.GetByEndpoint(controller, method, action);
        }
    }
}
