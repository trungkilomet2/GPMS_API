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
        private readonly IBaseRepositories<Order> _orderRepo;

        public CommentServices(IBaseRepositories<Comment> commentRepo, IBaseRepositories<Order> orderRepo)
        {
            _commentRepo = commentRepo ?? throw new ArgumentNullException(nameof(commentRepo));
            _orderRepo = orderRepo;
        }

        public Task<Comment> CreateComment(Comment entity)
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

        public async Task DeleteComment(int id, int userId)
        {
            if (id <= 0)
                throw new Exception("Invalid comment id");

            var comment = await _commentRepo.GetById(id);
            if (comment == null)
                throw new Exception("Comment not found");

            if (comment.fromUserId != userId)
                throw new Exception("You can only update your own comment");

            if (comment == null)
                throw new Exception($"Comment with id {id} does not exist");

            if (string.IsNullOrWhiteSpace(comment.Content))
                throw new Exception("Cannot delete empty comment");

            await _commentRepo.Delete(id);
        }

        public async Task<IEnumerable<Comment>> GetCommentById(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order id");

            var order = await _orderRepo.GetById(orderId);

            if (order == null)
                throw new Exception($"Order with id {orderId} does not exist");
            var data = await _commentRepo.GetAll(orderId);   
            if(!data.Any())
                {
                throw new Exception("No comments found for this order");
            }
            return data;
        }

        public async Task<Comment> UpdateComment(Comment entity, int userId)
        {
            var existing = await _commentRepo.GetById(entity.Id);
            if (existing == null)
                throw new Exception("Comment not found");

            if (existing.fromUserId != userId)
                throw new Exception("You can only update your own comment");
            var data = await _commentRepo.Update(entity);
            if (data == null)
            {
                throw new Exception("Comment not found");
            }
            return data;
        }
    }
}
