using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerUserStatusRepository : IBaseRepositories<UserStatus>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerUserStatusRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public Task<UserStatus> Create(UserStatus entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserStatus>> GetAll(object? obj)
        {
            throw new NotImplementedException();
        }

        public Task<UserStatus> GetById(object id)
        {
            var data = _context.U_STATUS.FirstOrDefault(x => x.US_ID == (int)id);
            return Task.FromResult(_mapper.Map<UserStatus>(data));
        }

        public Task<UserStatus> Update(UserStatus entity)
        {
            throw new NotImplementedException();
        }
    }
}
