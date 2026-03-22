using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Entities.GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplateController : ControllerBase
    {
        private readonly ITemplateRepositories _templateService;

        public TemplateController(ITemplateRepositories templateService)
        {
            _templateService = templateService;
        }

        [HttpGet("template/list")]
        public async Task<ActionResult<RestDTO<IEnumerable<TemplateViewDTO>>>> GetAll()
        {
            var templates = await _templateService.GetAll();
            var data = templates.Select(x => new TemplateViewDTO
            {
                TemplateId = x.Id,
                TemplateName = x.Name,
                Steps = x.Steps.Select(s => new TemplateStepViewDTO
                {
                    StepId = s.Id,
                    StepOrder = s.Order,
                    PartName = s.PartName
                }).ToList()
            });
            return Ok(new RestDTO<IEnumerable<TemplateViewDTO>> { Data = data });
        }

        [HttpPost("template/create")]
        public async Task<ActionResult<RestDTO<TemplateViewDTO>>> Create([FromBody] CreateProductionTemplateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                var template = await _templateService.Create(new TemplateDefinition
                {
                    Name = dto.TemplateName,
                    Steps = dto.Steps.Select(s => new TemplateStepDefinition
                    {
                        Order = s.StepOrder,
                        PartName = s.PartName
                    }).ToList()
                });

                return StatusCode(StatusCodes.Status201Created, new RestDTO<TemplateViewDTO>
                {
                    Data = new TemplateViewDTO
                    {
                        TemplateId = template.Id,
                        TemplateName = template.Name,
                        Steps = template.Steps.Select(s => new TemplateStepViewDTO
                        {
                            StepId = s.Id,
                            StepOrder = s.Order,
                            PartName = s.PartName
                        }).ToList()
                    }
                });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails { Detail = ex.Message, Status = 400 });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Detail = ex.Message, Status = 500 });
            }
        }
    }
}