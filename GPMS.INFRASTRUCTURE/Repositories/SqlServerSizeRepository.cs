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
    public class SqlServerSizeRepository : IBaseRepositories<Size>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerSizeRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public Task<Size> Create(Size entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Size>> GetAll(object? obj)
        {
            var data = await _context.SIZE.ToListAsync();
            return _mapper.Map<IEnumerable<Size>>(data);
        }

        public async Task<Size> GetById(object id)
        {
            var data = await _context.SIZE.FirstOrDefaultAsync(x => x.SIZE_ID == (int)id);
            return _mapper.Map<Size>(data);
        }

        public Task<Size> Update(Size entity)
        {
            throw new NotImplementedException();
        }
    }
}
