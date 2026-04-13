using GPMS.APPLICATION.ContextRepo;
using GPMS.INFRASTRUCTURE.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class DbContextUnitOfWork : IUnitOfWork
    {
        private readonly GPMS_SYSTEMContext _context;

        public DbContextUnitOfWork(GPMS_SYSTEMContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cts = default)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));

            using (var transaction = await _context.Database.BeginTransactionAsync(cts))
            {
                try
                {
                    await action();
                    await transaction.CommitAsync(cts);
                }
                catch(Exception ex)
                {
                    await transaction.RollbackAsync(cts);
                    throw ;
                }
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
         return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
