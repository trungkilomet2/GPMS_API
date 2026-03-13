using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly IProductionRepositories _productionService;

        public ProductionController(IProductionRepositories productionService)
        {
            _productionService = productionService;
        }

        [HttpPost("create-production")]
        public async Task<ActionResult<MessageResponseDTO<Production>>> CreateProduction([FromBody] CreateProductionDTO dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _productionService.CreateProduction(new Production
                    {
                        PmId = dto.PmId,
                        OrderId = dto.OrderId,
                        StartDate = dto.StartDate,
                        EndDate = dto.EndDate,
                        StatusId = dto.StatusId
                    });

                    return Ok(new MessageResponseDTO<Production>
                    {
                        MessageCode = Message_Codes.PROD_PLAN_CREATED,
                        MessageContent = Message_Contents.PRODUCTION_PLAN_CREATED,
                        Data = result
                    });
                } else
                {
                    var detail = new ValidationProblemDetails(ModelState);
                    return BadRequest(new MessageResponseDTO<Production>
                    {
                        MessageCode = Message_Codes.SYS_ACTION_FAILED,
                        MessageContent = detail.Detail ?? "Invalid input data."
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageResponseDTO<Production>
                {
                    MessageCode = Message_Codes.ORD_CANCEL_SUCCESS,
                    MessageContent = ex.Message
                });
            }
        }

        [HttpGet("get-list-production")]
        public async Task<ActionResult<IEnumerable<Production>>> GetList()
            => Ok(await _productionService.GetProductionList());

        [HttpGet("production-detail/{id:int}")]
        public async Task<ActionResult<Production>> GetDetail(int id)
            => Ok(await _productionService.GetProductionDetail(id));

        [HttpPatch("{id:int}/revision-request")]
        public async Task<ActionResult<Production>> RequestRevision(int id)
            => Ok(await _productionService.RequestProductionRevision(id));

        [HttpPatch("{id:int}/deny")]
        public async Task<ActionResult<Production>> Deny(int id, [FromBody] RejectProductionDTO dto)
            => Ok(await _productionService.DenyProduction(id, dto.UserId, dto.Reason));

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Production>> Update(int id, [FromBody] UpdateProductionDTO dto)
            => Ok(await _productionService.UpdateProduction(id, new Production
            {
                PmId = dto.PmId,
                OrderId = dto.OrderId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                StatusId = dto.StatusId
            }));

        [HttpGet("plans/pending")]
        public async Task<ActionResult<IEnumerable<Production>>> GetPendingPlans()
            => Ok(await _productionService.GetPendingProductionPlans());

        [HttpGet("plans")]
        public async Task<ActionResult<IEnumerable<Production>>> GetPlanList()
            => Ok(await _productionService.GetProductionPlanList());

        [HttpPost("{id:int}/plans/config")]
        public async Task<ActionResult<Production>> ConfigPlan(int id, [FromBody] List<ProductionPartDTO> dto)
            => Ok(await _productionService.ConfigProductionPlan(id, dto.Select(x => new ProductionPart
            {
                PartName = x.PartName,
                TeamLeaderId = x.TeamLeaderId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Cpu = x.Cpu,
                StatusId = x.StatusId
            })));

        [HttpGet("{id:int}/plans/detail")]
        public async Task<ActionResult<Production>> GetPlanDetail(int id)
            => Ok(await _productionService.GetProductionPlanDetail(id));

        [HttpPatch("{id:int}/plans/deny")]
        public async Task<ActionResult<Production>> DenyPlan(int id, [FromBody] RejectProductionDTO dto)
            => Ok(await _productionService.DenyProductionPlan(id, dto.UserId, dto.Reason));
    }
}
