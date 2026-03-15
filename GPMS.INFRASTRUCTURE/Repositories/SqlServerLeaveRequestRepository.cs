using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Constants;
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
            var query = _context.LEAVE_REQUEST
                .Include(lr => lr.USER)
                .Include(lr => lr.LRS)
                .AsQueryable();

            if (obj is int userId)
                query = query.Where(lr => lr.USER_ID == userId);

            var data = await query.ToListAsync();
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

        public async Task<LeaveRequest> Update(LeaveRequest entity)
        {
            var existing = await _context.LEAVE_REQUEST
                .FirstOrDefaultAsync(lr => lr.LR_ID == entity.Id);

            if (existing is null)
                throw new KeyNotFoundException($"Leave request with id '{entity.Id}' not found.");

            var status = await _context.LR_STATUS
                .FirstOrDefaultAsync(s => s.LRS_ID == entity.StatusId);

            if (status is null)
                throw new InvalidOperationException($"Status with id '{entity.StatusId}' not found in system.");

            existing.LRS_ID = status.LRS_ID;
            existing.DENY_CONTENT = entity.DenyContent;  
            existing.DATE_REPLY = entity.DateReply;

            await _context.SaveChangesAsync();

            return _mapper.Map<LeaveRequest>(existing);
        }

        public Task<LeaveRequest> Create(LeaveRequest entity) => throw new NotImplementedException();
        public Task Delete(object id) => throw new NotImplementedException();
    }
}