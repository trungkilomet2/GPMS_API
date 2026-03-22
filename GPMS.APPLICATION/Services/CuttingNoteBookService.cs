using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System.ComponentModel.DataAnnotations;

namespace GPMS.APPLICATION.Services
{
    public class CuttingNotebookService : ICuttingNotebookRepositories
    {
        private readonly IBaseRepositories<CuttingNotebook> _notebookRepo;
        private readonly IBaseRepositories<CuttingNotebookLog> _logRepo;
        private readonly IBaseRepositories<Production> _productionRepo;
        private readonly IBaseRepositories<User> _userRepo;

        public CuttingNotebookService(
            IBaseRepositories<CuttingNotebook> notebookRepo,
            IBaseRepositories<CuttingNotebookLog> logRepo,
            IBaseRepositories<Production> productionRepo,
            IBaseRepositories<User> userRepo)
        {
            _notebookRepo = notebookRepo;
            _logRepo = logRepo;
            _productionRepo = productionRepo;
            _userRepo = userRepo;
        }

        public async Task<CuttingNotebook> CreateNotebook(CuttingNotebook entity)
        {
            _ = await _productionRepo.GetById(entity.ProductionId) ?? throw new ValidationException("Production không tồn tại");
            if (entity.MarkerLength <= 0 || entity.FabricWidth <= 0) throw new ValidationException("Chiều dài sơ đồ và khổ vải phải > 0");
            return await _notebookRepo.Create(entity);
        }

        public Task<CuttingNotebook> GetNotebook(int notebookId) => _notebookRepo.GetById(notebookId);

        public Task<IEnumerable<CuttingNotebook>> GetByProduction(int productionId) => _notebookRepo.GetAll(productionId);

        public async Task<CuttingNotebookLog> CreateLog(CuttingNotebookLog entity)
        {
            _ = await _notebookRepo.GetById(entity.NotebookId) ?? throw new ValidationException("Sổ cắt không tồn tại");
            _ = await _userRepo.GetById(entity.UserId) ?? throw new ValidationException("User không tồn tại");
            return await _logRepo.Create(entity);
        }

        public Task<IEnumerable<CuttingNotebookLog>> GetLogs(int notebookId) => _logRepo.GetAll(notebookId);
    }
}