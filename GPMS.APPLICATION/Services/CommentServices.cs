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

        public async Task<IEnumerable<Comment>> GetCommentById(string orderId)
        {
            var allComments = await _commentRepo.GetAll();
            var data = allComments.Where(o => o.ToOrder == orderId);
            return data;
        }
    }
}
