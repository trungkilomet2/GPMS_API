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
    public class SqlServerLeaveRequestRepository : IBaseRepositories<LeaveRequest>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerLeaveRequestRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<LeaveRequest>> GetAll(object? obj)
        {
            var data = await _context.LEAVE_REQUEST
                .Include(lr => lr.USER)
                .Include(lr => lr.LRS)
                .ToListAsync();

            return _mapper.Map<IEnumerable<LeaveRequest>>(data);
        }

        public async Task<LeaveRequest> GetById(object id)
        {
            var data = await _context.LEAVE_REQUEST
                .Include(lr => lr.USER)
                .Include(lr => lr.LRS)
                .FirstOrDefaultAsync(lr => lr.LR_ID == (int)id);

            return _mapper.Map<LeaveRequest>(data);
        }

        public Task<LeaveRequest> Create(LeaveRequest entity) => throw new NotImplementedException();
        public Task<LeaveRequest> Update(LeaveRequest entity) => throw new NotImplementedException();
        public Task Delete(object id) => throw new NotImplementedException();
    }
}