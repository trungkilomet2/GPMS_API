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
        private readonly IBaseRepositories<User> _userRepo;

        public CommentServices(IBaseRepositories<Comment> commentRepo, IBaseRepositories<Order> orderRepo, IBaseRepositories<User> userRepo)
        {
            _commentRepo = commentRepo ?? throw new ArgumentNullException(nameof(commentRepo));
            _orderRepo = orderRepo;
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public Task<Comment> CreateComment(int userId, Comment entity)
        {
            var exist = _userRepo.GetById(entity.fromUserId);
            if (exist == null) {
                throw new Exception("User not found");
            }
             var orderExist = _orderRepo.GetById(entity.toOrderId);
            if (orderExist == null)
            {
                throw new Exception("Order not found");
            }
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(entity.Content))
                throw new Exception("Content cannot be empty");
            if (entity.fromUserId !=userId)
                throw new Exception("You can only create comment for yourself");
            var data = _commentRepo.Create(entity);
            return data;
        }

        public async Task DeleteComment(int CommentId, int userId)
        {
            if (CommentId <= 0)
                throw new Exception("Invalid comment id");

            var comment = await _commentRepo.GetById(CommentId);
            if (comment == null)
                throw new Exception("Comment not found");

            if (comment.fromUserId != userId)
                throw new Exception("You can only delete your own comment");

            if (comment == null)
                throw new Exception($"Comment with id {CommentId} does not exist");

            if (string.IsNullOrWhiteSpace(comment.Content))
                throw new Exception("Cannot delete empty comment");

            await _commentRepo.Delete(CommentId);
        }


        public async Task<IEnumerable<Comment>> GetCommentById(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order id");

            var order = await _orderRepo.GetById(orderId);

            if (order == null)
                throw new Exception($"Order does not exist");
            var data = await _commentRepo.GetAll(orderId);   
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
