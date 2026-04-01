using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Entities.GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.Repositories
{
    public interface ITemplateRepositories
    {
        Task<TemplateDefinition> Create(TemplateDefinition entity);
        Task Delete(int templateId);
        Task<IEnumerable<TemplateDefinition>> GetAll();
    }
}