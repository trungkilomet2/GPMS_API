using GPMS.APPLICATION.ContextRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.UnitOfWorks
{
    public interface IAccountRegisterUnitOfWork
    {
        IBaseAccountRepositories AccountRepo { get; }
        IBaseUserRoleRepo UserRoleRepo { get; }
        Task BeginTransactionAsync();
        Task SaveChangesAsync();
        Task CancelAsync();
    }
}
