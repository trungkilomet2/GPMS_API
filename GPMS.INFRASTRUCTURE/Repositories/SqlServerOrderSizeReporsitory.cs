using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
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

        public async Task<OrderSize> GetById(object id)
        {
            if (id is not int orderSizeId) return null!;
            var data = await _context.ORDER_SIZE.FirstOrDefaultAsync(x => x.OD_ID == orderSizeId);
            return data is null ? null! : _mapper.Map<OrderSize>(data);
        }

        public async Task<OrderSize> Update(OrderSize entity)
        {
            var data = await _context.ORDER_SIZE.FirstOrDefaultAsync(x => x.OD_ID == entity.Id);
            if (data is null) throw new KeyNotFoundException("OrderSize not found");

            data.SIZE_ID = entity.SizeId;
            data.COLOR = entity.Color;
            data.QUANTITY = entity.Quantity;
            data.OSS_ID = entity.OrderSizeStatusId;

            await _context.SaveChangesAsync();
            return _mapper.Map<OrderSize>(data);
        }
    }
}
