using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IUnitOfWork
    {
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
        /// <summary>
        /// Return number state entities has been change in database after save changes. 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
