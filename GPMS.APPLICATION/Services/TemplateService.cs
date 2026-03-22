using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Entities.GPMS.DOMAIN.Entities;
using System.ComponentModel.DataAnnotations;

namespace GPMS.APPLICATION.Services
{
    public class TemplateService : ITemplateRepositories
    {
        private readonly IBaseRepositories<TemplateDefinition> _templateRepo;

        public TemplateService(IBaseRepositories<TemplateDefinition> templateRepo)
        {
            _templateRepo = templateRepo;
        }

        public async Task<TemplateDefinition> Create(TemplateDefinition entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Name)) throw new ValidationException("Tên template là bắt buộc");
            if (entity.Steps is null || entity.Steps.Count == 0) throw new ValidationException("Template phải có ít nhất một công đoạn");
            return await _templateRepo.Create(entity);
        }

        public Task<IEnumerable<TemplateDefinition>> GetAll() => _templateRepo.GetAll(null);
    }
}