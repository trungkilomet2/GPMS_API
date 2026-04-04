using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
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
    public class SqlServerWorkerRepository : IBaseWorkerRepository
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerWorkerRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<User> AssignWorkerRole(User entity)
        {
            var existing = await _context.USER
                .Include(u => u.WS)
                .FirstOrDefaultAsync(u => u.USER_ID == entity.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Nhân viên '{entity.Id}' không tìm thấy");
            if (entity.WorkerSkills != null)
            {
                existing.WS.Clear();
                foreach (var role in entity.WorkerSkills)
                {
                    var roleEntity = await _context.WORKER_SKILL
                        .FirstOrDefaultAsync(r => r.WS_ID == role.Id);

                    if (roleEntity != null)
                        existing.WS.Add(roleEntity);
                }
            }

            await _context.SaveChangesAsync();

            return _mapper.Map<User>(existing);
        }

        public async Task<User> Create(User entity)
        {
            var userEntity = _mapper.Map<USER>(entity);
            var userNameExists = await _context.USER.Where(u => u.USERNAME.Equals(entity.UserName)).FirstOrDefaultAsync();
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
            if(obj is int id)
            {
                var workers = await _context.USER
                      .Include(u => u.ROLE)
                      .Include(u => u.WS)
                      .Include(u => u.US)
                      .Where(u => u.MANAGER_ID == id && u.ROLE.Any(r => r.NAME == Roles_Constants.PM ||
                      r.NAME == Roles_Constants.Worker)).ToListAsync();
                return _mapper.Map<IEnumerable<User>>(workers);
            }
            var users = await _context.USER
                              .Include(u => u.ROLE)
                              .Include(u => u.WS)
                              .Include(u => u.US)
                              .Where(u => u.ROLE.Any(r => r.NAME == Roles_Constants.PM ||
                              r.NAME == Roles_Constants.Worker)).ToListAsync();
            
            // Insert by TrungNT 26-03-2026
            // Lấy danh sách người phụ trách production đấy
            if(obj is WorkerByManagerDTO worker)
            {   
                var workerByManager = await _context.USER.Include(u => u.ROLE)
                              .Include(u => u.WS)
                              .Include(u => u.US)
                              .Where(u=> u.ROLE.Any(r => r.NAME == Roles_Constants.Worker) && u.MANAGER_ID == worker.ManagerId).ToListAsync();
                
                var pm = _context.USER.Include(u => u.WS)
                              .Include(u => u.US).Where(u => u.USER_ID == worker.ManagerId).FirstOrDefault();

                workerByManager.Add(pm);

                return _mapper.Map<List<User>>(workerByManager);
            }

            return _mapper.Map<List<User>>(users);
        }

        public async Task<User> GetWorkerById(int id)
        {
            var users = await _context.USER
                              .Include(u => u.ROLE)
                              .Include(u => u.WS)
                              .Include(u => u.US)
                              .Where(u => u.USER_ID == id && u.ROLE.Any(r => r.NAME == Roles_Constants.PM ||
                              r.NAME == Roles_Constants.Worker)).FirstOrDefaultAsync();
            return _mapper.Map<User>(users);
        }

        #region Insert by TrungNT | 22-03-2026
        // Insert by TrungNT
        public async Task<IEnumerable<WorkerSkill>> GetWorkerSkillByUserId(int userId)
        {   
            var workerSkill = await _context.USER
                .Include(ws => ws.WS).Where(u => u.USER_ID == userId).SelectMany(u => u.WS).ToListAsync();

            return _mapper.Map<IEnumerable<WorkerSkill>>(workerSkill);
        }

        #endregion

        public async Task<User> Update(User entity)
        {
            var existing = await _context.USER
                .Include(u => u.ROLE)
                .FirstOrDefaultAsync(u => u.USER_ID == entity.Id);

            if (existing == null)
                throw new KeyNotFoundException($"Employee '{entity.Id}' not found");

            existing.FULLNAME = entity.FullName;
            existing.MANAGER_ID = entity.ManagerId;

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
