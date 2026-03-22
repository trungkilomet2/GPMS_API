using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Entities.GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerTemplateRepository : IBaseRepositories<TemplateDefinition>
    {
        private readonly GPMS_SYSTEMContext _context;

        public SqlServerTemplateRepository(GPMS_SYSTEMContext context)
        {
            _context = context;
        }

        public async Task<TemplateDefinition> Create(TemplateDefinition entity)
        {
            var dbTemplate = new TEMPLATE { TEMPLATE_NAME = entity.Name };
            await _context.TEMPLATE.AddAsync(dbTemplate);
            await _context.SaveChangesAsync();

            var steps = entity.Steps.Select(x => new TEMPLATE_STEP
            {
                TEMPLATE_ID = dbTemplate.TEMPLATE_ID,
                PRODUCTION_PART_NAME = x.PartName,
                STEP_ORDER = x.Order
            });

            await _context.TEMPLATE_STEP.AddRangeAsync(steps);
            await _context.SaveChangesAsync();
            return await GetById(dbTemplate.TEMPLATE_ID);
        }

        public Task Delete(object id) => throw new NotImplementedException();

        public async Task<IEnumerable<TemplateDefinition>> GetAll(object? obj)
        {
            var data = await _context.TEMPLATE.Include(x => x.TEMPLATE_STEP).OrderByDescending(x => x.TEMPLATE_ID).ToListAsync();
            return data.Select(MapTemplate);
        }

        public async Task<TemplateDefinition> GetById(object id)
        {
            if (id is not int templateId) return null;
            var data = await _context.TEMPLATE.Include(x => x.TEMPLATE_STEP).FirstOrDefaultAsync(x => x.TEMPLATE_ID == templateId);
            return data is null ? null : MapTemplate(data);
        }

        public Task<TemplateDefinition> Update(TemplateDefinition entity) => throw new NotImplementedException();

        private static TemplateDefinition MapTemplate(TEMPLATE x)
        {
            return new TemplateDefinition
            {
                Id = x.TEMPLATE_ID,
                Name = x.TEMPLATE_NAME,
                Steps = x.TEMPLATE_STEP.OrderBy(s => s.STEP_ORDER)
                    .Select(s => new TemplateStepDefinition { Id = s.STEP_ID, Order = s.STEP_ORDER, PartName = s.PRODUCTION_PART_NAME })
                    .ToList()
            };
        }
    }
}