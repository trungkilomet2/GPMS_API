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
    public class SqlServerOrderSizeReporsitory : IBaseRepositories<OrderSize>
    {

        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;



        public SqlServerOrderSizeReporsitory(GPMS_SYSTEMContext context,IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public Task<OrderSize> Create(OrderSize entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OrderSize>> GetAll(object? obj)
        {
            if(obj is int orderId)
            {
                var data = await _context.ORDER_SIZE.Where(x => x.ORDER_ID == orderId).ToListAsync();   
                return data.Select(x => _mapper.Map<OrderSize>(x)).ToList();
            }
            return null;
        }

        public Task<OrderSize> GetById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<OrderSize> Update(OrderSize entity)
        {
            throw new NotImplementedException();
        }
    }
}
