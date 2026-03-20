using AutoMapper;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerPermissionRepository : IPermissionRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerPermissionRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public IEnumerable<PermissionEntry> GetAll()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetRoleMap()
        {
            return _context.ROLE
                .AsNoTracking()
                .ToDictionary(r => r.ROLE_ID.ToString(), r => r.NAME);
        }
    }
}
