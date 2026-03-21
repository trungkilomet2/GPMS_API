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
    public class SqlServerProductionPartAssigneeRepository : IBaseProductionPartAssignRepositories
    {

        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionPartAssigneeRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<ProductionPart> AssignWorkers(int partId, IEnumerable<int> workerIds)
        {
            var dbEntity = await _context.P_PART
                .Include(x => x.USER)
                .FirstOrDefaultAsync(x => x.PP_ID == partId);

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
            return await GetById(partId);
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
            var data = await _context.PART_WORK_LOG.FirstOrDefaultAsync(x => x.WL_ID == workLogId);
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
            return _mapper.Map<ProductionPartWorkLog>(log);
        }

        public async Task<ProductionPart> GetById(object id)
        {
            if (id is not int partId)
            {
                throw new Exception("Id không hợp lệ");
            }

            var data = await _context.P_PART
                .Include(x => x.PPS)
                .Include(x => x.USER)
                .FirstOrDefaultAsync(x => x.PP_ID == partId);

            return data is null ? null : ToDomain(data);
        }

        private ProductionPart ToDomain(P_PART source)
        {
            var part = _mapper.Map<ProductionPart>(source);
            part.AssigneeIds = source.USER.Select(x => x.USER_ID).ToList();
            return part;
        }
        public async Task<ProductionPart> RemoveWorker(int partId, int workerId)
        {
            var dbEntity = await _context.P_PART
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


    }
}
