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
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerCommentRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public Task<Comment> Create(Comment entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Comment>> GetAll(object? obj)
        {
            List<UO_COMMENT> orderComments = new List<UO_COMMENT>();

            if (obj is int orderId)
            {
                orderComments = await _context.UO_COMMENT.Where(u => u.TO_ORDER == orderId).ToListAsync();
            }

            return _mapper.Map<IEnumerable<Comment>>(orderComments);
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
