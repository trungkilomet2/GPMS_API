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
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(entity.Content))
                throw new ArgumentException("Content cannot be empty");
            if (entity.fromUserId <= 0)
                throw new ArgumentException("Invalid fromUserId");
            if (entity.toOrderId <= 0)
                throw new ArgumentException("Invalid toOrderId");
            entity.SendDateTime = DateTime.UtcNow;
            var data = _commentRepo.Create(entity);
            return data;
        }

        public async Task Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid comment id");

            var comment = await _commentRepo.GetById(id);

            if (comment == null)
                throw new KeyNotFoundException($"Comment with id {id} does not exist");

            if (string.IsNullOrWhiteSpace(comment.Content))
                throw new InvalidOperationException("Cannot delete empty comment");

            await _commentRepo.Delete(id);
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
