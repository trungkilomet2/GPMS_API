using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Constants;
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
    public class SqlServerWorkerRepository : IBaseRepositories<User>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerWorkerRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<User> Create(User entity)
        {
            var userEntity = _mapper.Map<USER>(entity);
            var userNameExists = await _context.USER.Where(u => u.UserName.Equals(entity.UserName)).FirstOrDefaultAsync();
            if (userNameExists != null)
                {
                throw new Exception("Username already exists");
            }
            userEntity.ROLE.Clear();
            await _context.USER.AddAsync(userEntity);
            await _context.SaveChangesAsync();

            if (entity.Roles != null)
            {
                foreach (var role in entity.Roles)
                {
                    var roleEntity = await _context.ROLE
                        .FirstOrDefaultAsync(r => r.ROLE_ID == role.Id);

                    if (roleEntity != null)
                        userEntity.ROLE.Add(roleEntity);
                }
            }

            await _context.SaveChangesAsync();

            return _mapper.Map<User>(userEntity);
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<User>> GetAll(object? obj)
        {
            var users = await _context.USER
                              .Include(u => u.ROLE)
                              .Include(u => u.WR)
                              .Include(u => u.US)
                              .Where(u => u.ROLE.Any(r => r.NAME == Roles_Constants.PM || r.NAME == Roles_Constants.Team_Leader ||
                              r.NAME == Roles_Constants.Worker || r.NAME == Roles_Constants.KCS)).ToListAsync();
            return _mapper.Map<List<User>>(users);
        }

        public async Task<User> GetById(object id)
        {
            var users = await _context.USER
                              .Include(u => u.ROLE)
                              .Include(u => u.WR)
                              .Include(u => u.US)
                              .Where(u => u.USER_ID == (int)id && u.ROLE.Any(r => r.NAME == Roles_Constants.PM || r.NAME == Roles_Constants.Team_Leader ||
                              r.NAME == Roles_Constants.Worker || r.NAME == Roles_Constants.KCS)).FirstOrDefaultAsync();
            return _mapper.Map<User>(users);
        }

        public async Task<User> Update(User entity)
        {
            var existing = await _context.USER
                .Include(u => u.ROLE)
                .FirstOrDefaultAsync(u => u.USER_ID == entity.Id);

            if (existing == null)
                throw new KeyNotFoundException($"Employee '{entity.Id}' not found");

            existing.FULLNAME = entity.FullName;
            existing.US_ID = entity.StatusId;

            if (entity.Roles != null)
            {
                existing.ROLE.Clear();

                foreach (var role in entity.Roles)
                {
                    var roleEntity = await _context.ROLE
                        .FirstOrDefaultAsync(r => r.ROLE_ID == role.Id);

                    if (roleEntity != null)
                        existing.ROLE.Add(roleEntity);
                }
            }

            await _context.SaveChangesAsync();

            return _mapper.Map<User>(existing);
        }
    }
}
