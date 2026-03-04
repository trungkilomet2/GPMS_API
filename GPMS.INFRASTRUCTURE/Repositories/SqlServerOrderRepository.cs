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
    public class SqlServerOrderRepository : IBaseRepositories<Order>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerOrderRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<Order>> GetAll(object? obj)
        {
            if (obj is int userId)
            {
                var filtered = await _context.ORDER
                    .Include(o => o.OS)
                    .Where(o => o.USER_ID == userId)
                    .ToListAsync();
                return _mapper.Map<IEnumerable<Order>>(filtered);
            }

            var data = await _context.ORDER.Include(o => o.OS).ToListAsync();
            return _mapper.Map<IEnumerable<Order>>(data);
        }

        public async Task<Order> GetById(object id)
        {
            var data = await _context.ORDER.Include(o => o.OS)
                .Where(o => o.ORDER_ID == (int)id)
                .FirstOrDefaultAsync();
            return _mapper.Map<Order>(data);
        }

        public async Task<Order> Create(Order entity)
        {
            var orderEntity =  _mapper.Map<ORDER>(entity);
            await _context.ORDER.AddAsync(orderEntity);
            await _context.SaveChangesAsync();
            return _mapper.Map<Order>(orderEntity);
        }
        public Task<Order> Update(Order entity) => throw new NotImplementedException();
        public Task Delete(object id) => throw new NotImplementedException();
    }
}