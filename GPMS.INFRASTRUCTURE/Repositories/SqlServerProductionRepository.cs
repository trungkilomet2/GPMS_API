using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerProductionRepository : IBaseRepositories<Production>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Task<IEnumerable<Production>> GetAll(object? obj)
        {
            if (obj is null)
            {
                var data = _context.PRODUCTION.Include(p => p.ORDER).Include(p=> p.PM);
                return Task.FromResult(_mapper.Map<IEnumerable<Production>>(data));
            }
            if(obj is Order order)
            {
                var data =  _context.PRODUCTION.Where(p => p.ORDER_ID == order.Id && p.PS_ID != ProductionStatus_Constants.Reject_ID).ToList(); 
                return Task.FromResult(_mapper.Map<IEnumerable<Production>>(data));
            }

            throw new Exception("Đã có lỗi xảy ra trong hệ thống");
        }

        public Task<Production> GetById(object id)
        {
            if (id is int)
            {
                var data = _context.PRODUCTION.Where(p => p.PRODUCTION_ID == (int)id).FirstOrDefault();
                return data is null ? Task.FromResult<Production>(null) : Task.FromResult(_mapper.Map<Production>(data));
            }
            throw new Exception("Đã có lỗi xảy ra trong hệ thống");
        }

        public async Task<Production> Create(Production entity)
        {
            var pm_id = await _context.USER.Include(u => u.ROLE).Where(u => u.USER_ID == entity.PmId).FirstOrDefaultAsync();
            if (pm_id is null)
            {
                throw new Exception($"User with id '{entity.PmId}' not found.");
            }

            bool haveManagerWorker = _context.USER.Any(u => u.MANAGER_ID == pm_id.USER_ID);
            if(!haveManagerWorker)
            {
                throw new Exception($"PM '{entity.PmId}' đang không quản lý công nhân nào");
            }

            var isOrderExist = await _context.ORDER.AnyAsync(o => o.ORDER_ID == entity.OrderId);
            if (!isOrderExist)
            {
                throw new Exception($"User with id '{entity.OrderId}' not found.");
            }
            var production_database = _mapper.Map<PRODUCTION>(entity);
            _context.PRODUCTION.Add(production_database);
            await _context.SaveChangesAsync();
            return await GetById(production_database.PRODUCTION_ID) ?? throw new Exception("Xảy ra lỗi khi tạo đơn hàng");
        }

        public async Task<Production> Update(Production entity)
        {
            bool haveManagerWorker = _context.USER.Any(u => u.MANAGER_ID == entity.PmId);
            if (!haveManagerWorker)
            {
                throw new DBConcurrencyException($"PM ID = '{entity.PmId}' đang không quản lý công nhân nào");
            }
            PRODUCTION production_databse = await _context.PRODUCTION.FirstOrDefaultAsync(p=> p.PRODUCTION_ID == entity.Id);
            if(production_databse is null ) throw new DBConcurrencyException($"Không tìm thấy Production ID = '{entity.Id}'.");
            production_databse.PM_ID = entity.PmId;
            production_databse.ORDER_ID = entity.OrderId;
            production_databse.P_START_DATE = entity.StartDate;
            production_databse.P_END_DATE = entity.EndDate;
            production_databse.PS_ID = entity.StatusId;
            await _context.SaveChangesAsync();
            return await GetById(entity.Id) ?? throw new DBConcurrencyException("Cập Nhật Production Thất Bại.");
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

    }
}
