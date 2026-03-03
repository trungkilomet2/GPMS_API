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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<Comment> Create(Comment entity)
        {
            await _context.UO_COMMENT.AddAsync(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<Comment>(entity);
        }

        public async Task Delete(object id)
        {
            var data = await _context.UO_COMMENT.FindAsync(id);
            _context.UO_COMMENT.Remove(data);
            await _context.SaveChangesAsync();
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
            var data = await _context.UO_COMMENT.FindAsync(id);
            return _mapper.Map<Comment>(data);
        }

        public async Task<Comment> Update(Comment entity)
        {
            var data = await _context.UO_COMMENT.FindAsync(entity.Id);
            if(data != null)
            {
                data.CONTENT = entity.Content;
                data.SEND_DATETIME = DateTime.Parse(entity.SendDateTime);
                await _context.SaveChangesAsync();
            }
            return _mapper.Map<Comment>(data);

        }
       
    }
}
