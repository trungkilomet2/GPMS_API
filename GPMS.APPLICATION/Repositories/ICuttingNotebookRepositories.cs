using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.Repositories
{
    public interface ICuttingNotebookRepositories
    {
        Task<CuttingNotebook> CreateNotebook(CuttingNotebook entity);
        Task<CuttingNotebook> GetNotebook(int notebookId);
        Task<IEnumerable<CuttingNotebook>> GetByProduction(int productionId);
        Task<CuttingNotebookLog> CreateLog(CuttingNotebookLog entity);
        Task<IEnumerable<CuttingNotebookLog>> GetLogs(int notebookId);
    }
}