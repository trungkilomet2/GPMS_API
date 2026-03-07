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
    public class SqlServerMaterialRepository : IBaseRepositories<OMaterial>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerMaterialRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<OMaterial> Create(OMaterial entity)
        {
            var materialEntity = _mapper.Map<O_MATERIAL>(entity);
            await _context.O_MATERIAL.AddAsync(materialEntity);
            await _context.SaveChangesAsync();
            return _mapper.Map<OMaterial>(materialEntity);
        }

        public Task<IEnumerable<OMaterial>> GetAll(object? obj) => throw new NotImplementedException();
        public Task<OMaterial> GetById(object id) => throw new NotImplementedException();
        public Task<OMaterial> Update(OMaterial entity) => throw new NotImplementedException();
        public Task Delete(object id) => throw new NotImplementedException();
    }
}