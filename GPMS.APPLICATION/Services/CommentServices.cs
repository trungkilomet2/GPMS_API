using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class CommentServices : ICommentRepositories
    {
        private readonly IBaseRepositories<Comment> _commentRepo;

        public CommentServices(IBaseRepositories<Comment> commentRepo)
        {
            _commentRepo = commentRepo ?? throw new ArgumentNullException(nameof(commentRepo));
        }

        public Task<Comment> Create(Comment entity)
        {          
            var data = _commentRepo.Create(entity);
            return data;
        }

        public Task Delete(object id)
        {
            var data = _commentRepo.Delete(id);
            return data;
        }

        public async Task<IEnumerable<Comment>> GetCommentById(int orderId)
        {
            var data = await _commentRepo.GetAll(orderId);           
            return data;
        }

        public Task<Comment> Update(Comment entity)
        {
            var data = _commentRepo.Update(entity);
            return data;
        }
    }
}
