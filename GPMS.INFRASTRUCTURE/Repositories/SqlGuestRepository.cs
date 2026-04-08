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
    public class SqlGuestRepository : IBaseRepositories<Guest>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlGuestRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<Guest> Create(Guest entity)
        {
            var guestEntity = _mapper.Map<GUEST_ORDER>(entity);
            await _context.GUEST_ORDER.AddAsync(guestEntity);
            await _context.SaveChangesAsync();
            return _mapper.Map<Guest>(guestEntity);
        }

        public async Task Delete(object id)
        {
            throw new NotImplementedException();
        }           
        

        public Task<IEnumerable<Guest>> GetAll(object? obj)
        {
            throw new NotImplementedException();
        }

        public Task<Guest> GetById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<Guest> Update(Guest entity)
        {
            throw new NotImplementedException();
        }
    }
}
