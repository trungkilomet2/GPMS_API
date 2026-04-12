using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerProductionPartOrderSizeRepository : IBaseRepositories<ProductionPartOrderSize>
    {

        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionPartOrderSizeRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductionPartOrderSize> Create(ProductionPartOrderSize entity)
        {
            P_PART_ORDER_SIZE convert = _mapper.Map<P_PART_ORDER_SIZE>(entity);
            var data = await _context.P_PART_ORDER_SIZE.AddAsync(convert);
            return entity;
        }


        public async Task Delete(object id)
        {
            var deleteData = await _context.P_PART_ORDER_SIZE.Where(u => u.PPOS_ID == (int)id).FirstOrDefaultAsync();
            if(deleteData is null)
            {
                throw new ValidationException("Không tồn tại dữ liệu trong hệ thống");
            }
            _context.P_PART_ORDER_SIZE.Remove(deleteData);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductionPartOrderSize>> GetAll(object? obj)
        {
            // Lấy tất cả danh sách worklog theo partId danh sách theo partID
            if (obj is int partid)
            {
                var data = await _context.P_PART_ORDER_SIZE.Include(pos=> pos.USER).Where(x => x.PP_ID == partid).ToListAsync();

                return _mapper.Map<IEnumerable<ProductionPartOrderSize>>(data);
            }
            return null;
        }

        public async Task<ProductionPartOrderSize> GetById(object id)
        {
            if (id is int partOrderSizeId)
            {
                var data = await _context.P_PART_ORDER_SIZE.Include(pos => pos.USER).FirstOrDefaultAsync(x => x.PPOS_ID == (int)id);
                return data is null ? null : ToDomain(data);
            }
            return null;
        }

        public Task<ProductionPartOrderSize> Update(ProductionPartOrderSize entity)
        {
            throw new NotImplementedException();
        }


        private ProductionPartOrderSize ToDomain(P_PART_ORDER_SIZE source)
        {
            var part = _mapper.Map<ProductionPartOrderSize>(source);
            part.AssigneeIds = source.USER.Select(x => x.USER_ID).ToList();
            return part;
        }
    }
}
