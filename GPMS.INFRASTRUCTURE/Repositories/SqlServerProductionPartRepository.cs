using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{

    public class SqlServerProductionPartRepository : IBaseRepositories<ProductionPart>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionPartRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductionPart>> GetAll(object? obj)
        {
            IQueryable<P_PART> query = _context.P_PART.Include(x => x.PPS).Include(x => x.USER);
            if (obj is int productionId)
            {
                query = query.Where(x => x.PRODUCTION_ID == productionId);
            }
            var data = await query.ToListAsync();
            return data.Select(ToDomain);
        }

        public async Task<ProductionPart> GetById(object id)
        {
            if (id is not int partId)
            {
                throw new ValidationException("Id đầu vào không hợp lệ");
            }

            var data = await _context.P_PART
                .Include(x => x.PPS)
                .Include(x => x.USER)
                .FirstOrDefaultAsync(x => x.PP_ID == partId);

            return data is null ? null : ToDomain(data);
        }

        public async Task<ProductionPart> Create(ProductionPart entity)
        {
            try
            {

                var dbEntity = _mapper.Map<P_PART>(entity);
                await _context.P_PART.AddAsync(dbEntity);
                await _context.SaveChangesAsync();
                return await GetById(dbEntity.PP_ID);
            }
            catch (Exception ex)
            {
                throw new Exception($"Đã có lỗi xảy ra trong hệ thống: {ex.Message}");
            }
        }

        public async Task<ProductionPart> Update(ProductionPart entity)
        {
            var dbEntity = await _context.P_PART.FirstOrDefaultAsync(x => x.PP_ID == entity.Id);
            if (dbEntity is null)
            {
                throw new ValidationException("Không tồn tại Production Part trong hệ thống");
            }
            dbEntity.PART_NAME = entity.PartName;
            dbEntity.TEAM_LEADER_ID = entity.TeamLeaderId;
            dbEntity.START_DATE = entity.StartDate;
            dbEntity.END_DATE = entity.EndDate;
            dbEntity.CPU = entity.Cpu;
            dbEntity.PPS_ID = entity.StatusId;
            await _context.SaveChangesAsync();
            return await GetById(entity.Id);
        }

        // ????????
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

        public async Task Delete(object id)
        {
            if (id is int partId)
            {
                var dbEntity = await _context.P_PART.FirstOrDefaultAsync(x => x.PP_ID == partId);
                if (dbEntity is null)
                {
                   throw new ValidationException("Không tồn tại Production Part trong hệ thống");
                }
                _context.P_PART.Remove(dbEntity);
                await _context.SaveChangesAsync();
            }
        }

        private ProductionPart ToDomain(P_PART source)
        {
            var part = _mapper.Map<ProductionPart>(source);
            part.AssigneeIds = source.USER.Select(x => x.USER_ID).ToList();
            return part;
        }
    }
}
