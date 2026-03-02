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
    public class SqlServerCommentRepository : IBaseRepositories<Comment>
    {
        private readonly GPMS_SYSTEMContext context;
        private readonly IMapper mapper;

        public SqlServerCommentRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public Task<Comment> Create(Comment entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Comment>> GetAll()
        {
            var data = await context.UO_COMMENT.ToListAsync();
            return mapper.Map<IEnumerable<Comment>>(data);
        }

        public async Task<Comment> GetById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<Comment> Update(Comment entity)
        {
            throw new NotImplementedException();
        }
       
    }
}
