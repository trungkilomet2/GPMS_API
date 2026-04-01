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
    public class SqlServerOrderRejectRepository : IBaseRepositories<OrderRejectReason>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerOrderRejectRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<OrderRejectReason> Create(OrderRejectReason entity)
        {
            var uoOrderRejectReason = _mapper.Map<ORDER_REJECT_REASON>(entity);
            await _context.ORDER_REJECT_REASON.AddAsync(uoOrderRejectReason);
            return _mapper.Map<OrderRejectReason>(uoOrderRejectReason);
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrderRejectReason>> GetAll(object? obj)
        {
            throw new NotImplementedException();
        }

        public async Task<OrderRejectReason> GetById(object id)
        {
            var data = await _context.ORDER_REJECT_REASON.Where(o => o.ORDER_ID == (int)id).FirstOrDefaultAsync();
            return _mapper.Map<OrderRejectReason>(data);
        }

        public Task<OrderRejectReason> Update(OrderRejectReason entity)
        {
            throw new NotImplementedException();
        }
    }
}
