using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerOrderRepository : IBaseOrderRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerOrderRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            var data = await _context.ORDER
                .Include(o => o.OS)
                .ToListAsync();

            return _mapper.Map<IEnumerable<Order>>(data);
        }

        public async Task<Order?> GetOrderById(int id)
        {
            var data = await _context.ORDER
                .Include(o => o.OS)
                .Include(o => o.O_MATERIAL)
                .Include(o => o.O_TEMPLATE)
                .FirstOrDefaultAsync(o => o.ORDER_ID == id);

            if (data is null) return null;

            return _mapper.Map<Order>(data);
        }
    }
}