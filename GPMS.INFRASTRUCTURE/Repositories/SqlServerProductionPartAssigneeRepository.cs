using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerProductionPartAssigneeRepository : IBaseProductionPartAssignRepositories
    {

        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionPartAssigneeRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Assign worker cho production part size
        public async Task<ProductionPart> AssignWorkers(int partOrderSizeId, IEnumerable<int> workerIds)
        {
            var dbEntity = await _context.P_PART_ORDER_SIZE
                .Include(x => x.USER)
                .FirstOrDefaultAsync(x => x.PPOS_ID == partOrderSizeId);

            if (dbEntity is null)
            {
                return null;
            }

            dbEntity.USER.Clear();

            var workers = await _context.USER
                .Where(x => workerIds.Contains(x.USER_ID))
                .ToListAsync();

            foreach (var worker in workers)
            {
                dbEntity.USER.Add(worker);
            }

            await _context.SaveChangesAsync();
            return await GetById(dbEntity.PP_ID);
        }

        public async Task<ProductionPartWorkLog> CreateWorkLog(ProductionPartWorkLog entity)
        {
            var dbEntity = _mapper.Map<PART_WORK_LOG>(entity);
            _context.PART_WORK_LOG.Add(dbEntity);
            await _context.SaveChangesAsync();

            return await GetWorkLogById(dbEntity.WL_ID);
        }

        public async Task<ProductionPartWorkLog> GetWorkLogById(int workLogId)
        {
            var data = await _context.PART_WORK_LOG.Include(x => x.PPOS).FirstOrDefaultAsync(x => x.WL_ID == workLogId);
            return data is null ? null : _mapper.Map<ProductionPartWorkLog>(data);
        }

        public async Task<int> MarkWorkLogsReadOnlyAfterDate(DateTime date)
        {
            var logs = await _context.PART_WORK_LOG
                .Where(x => x.CREATE_DATE < date && x.IS_READ_ONLY != true)
                .ToListAsync();

            foreach (var log in logs)
            {
                log.IS_READ_ONLY = true;
            }

            await _context.SaveChangesAsync();
            return logs.Count;
        }

        public async Task<ProductionPartWorkLog> MarkWorkLogAsPaid(int workLogId)
        {
            var log = await _context.PART_WORK_LOG.FirstOrDefaultAsync(x => x.WL_ID == workLogId);
            if (log is null)
            {
                return null;
            }

            log.IS_PAYMENT = true;
            await _context.SaveChangesAsync();
            return await GetWorkLogById(workLogId);
        }

        // Lấy danh sách GetByID của production part order size
        public async Task<ProductionPart> GetById(object id)
        {
            if (id is not int partId)
            {
                throw new Exception("Id không hợp lệ");
            }

            var data = await _context.P_PART
                .FirstOrDefaultAsync(x => x.PP_ID == partId);

            return data is null ? null : ToDomain(data);
        }

        private ProductionPart ToDomain(P_PART source)
        {
            var part = _mapper.Map<ProductionPart>(source);
            return part;
        }
        public async Task<ProductionPart> RemoveWorker(int partId, int workerId)
        {
            var dbEntity = await _context.P_PART_ORDER_SIZE
                .Include(x => x.USER)
                .FirstOrDefaultAsync(x => x.PP_ID == partId);

            if (dbEntity is null)
            {
                return null;
            }

            var user = dbEntity.USER.FirstOrDefault(x => x.USER_ID == workerId);
            if (user is not null)
            {
                dbEntity.USER.Remove(user);
                await _context.SaveChangesAsync();
            }

            return await GetById(partId);
        }

        public async Task<IEnumerable<User>> ListWorkerWithPM(int pm_id, bool isPM)
        {   
            USER check_id = await _context.USER.Include(u => u.ROLE).Where(x => x.USER_ID == pm_id).FirstOrDefaultAsync();

            if (check_id is null)
            {
                throw new ValidationException("Không tồn tại người dùng trong hệ thống");
            }

            if (isPM)
            {

               List<USER> workers = await _context.USER
                .Include(u => u.ROLE)
                .Where(u => u.ROLE.Any(r => r.NAME.Equals(Roles_Constants.Worker)) && u.MANAGER_ID == pm_id)
                .ToListAsync();

                return _mapper.Map<IEnumerable<User>>(workers);
            }else
            {
                var owner = await _context.USER
                .Include(u => u.ROLE)
                .Include(u => u.WS)
                .Include(u => u.US)
                .FirstOrDefaultAsync(u => u.USER_ID == pm_id);

                if (owner == null || !owner.ROLE.Any(r => r.NAME == Roles_Constants.Owner))
                {
                    throw new KeyNotFoundException($"Owner với Id '{pm_id}' không tồn tại.");
                }

                var pms = await _context.USER
                    .Include(u => u.ROLE)
                    .Include(u => u.WS)
                    .Include(u => u.US)
                    .Where(u => u.MANAGER_ID == pm_id
                                && u.ROLE.Any(r => r.NAME == Roles_Constants.PM)
                                && !u.ROLE.Any(r => r.NAME == Roles_Constants.Admin || r.NAME == Roles_Constants.Customer))
                    .ToListAsync();

                var pmIds = pms.Select(x => x.USER_ID).ToList();
                var workers = await _context.USER
                    .Include(u => u.ROLE)
                    .Include(u => u.WS)
                    .Include(u => u.US)
                    .Where(u => pmIds.Contains(u.MANAGER_ID ?? 0)
                                && u.ROLE.Any(r => r.NAME == Roles_Constants.Worker)
                                && !u.ROLE.Any(r => r.NAME == Roles_Constants.Admin || r.NAME == Roles_Constants.Customer))
                    .ToListAsync();

                var hierarchyUsers = new List<USER> { owner };
                hierarchyUsers.AddRange(pms);
                hierarchyUsers.AddRange(workers);

                return _mapper.Map<IEnumerable<User>>(hierarchyUsers);

            }
        }

        
    }
}
