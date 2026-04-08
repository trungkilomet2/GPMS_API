using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    internal class SqlServerProductionPartOrderSizeRepository : IBaseRepositories<ProductionPartOrderSize>
    {

        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionPartOrderSizeRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Task<ProductionPartOrderSize> Create(ProductionPartOrderSize entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ProductionPartOrderSize>> GetAll(object? obj)
        {
            // Lấy tất cả danh sách worklog theo partId danh sách theo partID
            if (obj is int partid)
            {
                var data = await _context.P_PART_ORDER_SIZE.Where(x => x.PP_ID == partid).ToListAsync();
                return _mapper.Map<IEnumerable<ProductionPartOrderSize>>(data);
            }
            return null;
        }

        public async Task<ProductionPartOrderSize> GetById(object id)
        {
            if (id is int partOrderSizeId)
            {
                var data = await _context.P_PART_ORDER_SIZE.FirstOrDefaultAsync(x => x.PPOS_ID == (int)id);
                return _mapper.Map<ProductionPartOrderSize>(data);
            }
            return null;
        }

        public Task<ProductionPartOrderSize> Update(ProductionPartOrderSize entity)
        {
            throw new NotImplementedException();
        }
    }
}
