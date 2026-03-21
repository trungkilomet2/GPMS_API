using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerUserRoleRepository : IBaseUserRoleRepo
    {

        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerUserRoleRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Add Role to User in SQL
        public async Task AddUserRole(User user, string roleName)
        {
            if(user is null)
            {
                throw new Exception("Không tìm thấy tài khoản");
            }
            var role = await _context.ROLE.Where(r => r.NAME.Equals(roleName)).FirstOrDefaultAsync();
            if (role is null) {
                throw new Exception("Không tìm role");
            }

            USER sqlUser = await _context.USER.Where(u => u.USER_ID == user.Id).FirstOrDefaultAsync();
            sqlUser.ROLE.Add(role);
            await _context.SaveChangesAsync();  
        }

        public Task RemoveUserRole(User user, string roleName)
        {
            throw new NotImplementedException();
        }

        public async Task ReplaceUserRoles(User user, List<string> roleNames)
        {
            var sqlUser = await _context.USER
                .Include(u => u.ROLE)
                .Where(u => u.USER_ID == user.Id)
                .FirstOrDefaultAsync();

            if (sqlUser == null)
                throw new KeyNotFoundException($"User with ID {user.Id} not found.");

            sqlUser.ROLE.Clear();

            foreach (var roleName in roleNames)
            {
                var role = await _context.ROLE.Where(r => r.NAME == roleName).FirstOrDefaultAsync();
                if (role == null)
                    throw new KeyNotFoundException($"Role '{roleName}' not found.");
                sqlUser.ROLE.Add(role);
            }

            await _context.SaveChangesAsync();
        }
    }
}
