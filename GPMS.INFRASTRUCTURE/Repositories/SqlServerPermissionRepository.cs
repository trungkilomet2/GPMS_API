using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerPermissionRepository : IPermissionRepositories
    {
        private readonly GPMS_SYSTEMContext _context;

        public SqlServerPermissionRepository(GPMS_SYSTEMContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<PermissionEntry>> GetAll()
        {
            var rows = await _context.USER_AUTHORIZE.AsNoTracking().ToListAsync();
            return rows.Select(r => new PermissionEntry(
                r.CONTROLLER,
                r.METHOD,
                r.ACTION,
                r.ROLE_AUTHORIZE ?? string.Empty
            ));
        }

        public async Task<Dictionary<string, string>> GetRoleMap()
        {
            var roles = await _context.ROLE.AsNoTracking().ToListAsync();
            return roles.ToDictionary(r => r.ROLE_ID.ToString(), r => r.NAME);
        }
    }
}
