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
        private readonly IBaseRepositories<User> _userRepo;

        public CommentServices(IBaseRepositories<Comment> commentRepo, IBaseRepositories<User> userRepo)
        {
            _commentRepo = commentRepo ?? throw new ArgumentNullException(nameof(commentRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public async Task<Comment> CreateComment(Comment entity)
        {
            var result = await _userRepo.GetById(entity.fromUserId);
            if (result == null) 
            { 
                throw new Exception($"User with id {entity.fromUserId} does not exist");
            }
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(entity.Content))
                throw new ArgumentException("Content cannot be empty");
            if (entity.fromUserId <= 0)
                throw new ArgumentException("Invalid fromUserId");
            if (entity.toOrderId <= 0)
                throw new ArgumentException("Invalid toOrderId");
            entity.SendDateTime = DateTime.UtcNow;
            var data = await _commentRepo.Create(entity);
            return data;
        }

        public async Task DeleteComment(int id)
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

        public Task<Comment> UpdateComment(Comment entity)
        {
            var data = _commentRepo.Update(entity);
            if (data == null)
            {
                throw new Exception("Comment not found");
            }
            return data;
        }
    }
}
