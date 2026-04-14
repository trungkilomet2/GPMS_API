using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerPermissionRepository : IBasePermissionRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerPermissionRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<PermissionEntry>> GetAll()
        {
            var rows = await _context.USER_AUTHORIZE.AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<PermissionEntry>>(rows);
        }

        public async Task<PermissionEntry?> GetById(int id)
        {
            var row = await _context.USER_AUTHORIZE.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == id);
            return row == null ? null : _mapper.Map<PermissionEntry>(row);
        }

        public async Task<Dictionary<string, string>> GetRoleMap()
        {
            var roles = await _context.ROLE.AsNoTracking().ToListAsync();
            return roles.ToDictionary(r => r.ROLE_ID.ToString(), r => r.NAME);
        }

        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            var roles = await _context.ROLE.AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<Role>>(roles);
        }

        public async Task<bool> UpdateRoleAuthorize(int id, string? roleAuthorize)
        {
            var affected = await _context.USER_AUTHORIZE
                .Where(x => x.ID == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ROLE_AUTHORIZE, roleAuthorize));
            return affected > 0;
        }

        public async Task<PermissionEntry?> GetByEndpoint(string controller, string method, string action)
        {
            var row = await _context.USER_AUTHORIZE.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CONTROLLER == controller &&
                    x.METHOD == method &&
                    x.ACTION == action);
            return row == null ? null : _mapper.Map<PermissionEntry>(row);
        }
    }
}
