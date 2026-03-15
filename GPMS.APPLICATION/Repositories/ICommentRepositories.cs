using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface ICommentRepositories
    {
        Task<IEnumerable<Comment>> GetCommentById(int orderId);  
        Task<Comment> CreateComment(int userId,Comment entity);
        Task<Comment> UpdateComment(Comment entity, int userId);
        Task DeleteComment(int id, int userId);
    }
}
