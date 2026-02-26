using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IBaseRepository<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(object id);
        Task<T> Create(T entity);
        Task<T> Update(T entity);
        Task Delete(object id);
    }
}
