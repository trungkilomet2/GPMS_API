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

        [HttpPost("production/create")]
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
                        StatusId = ProductionStatus_Constants.Pending_ID
                    });
                    return Ok(new RestDTO<Production>
                    {
                        Data = result,
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(Url.Action(null,"production/create",result,Request.Scheme!),"self","POST")
                        }
                    });
                }
                else
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

        [HttpGet("production/list")]
        public async Task<ActionResult<IEnumerable<Production>>> GetList()
            => Ok(await _productionService.GetProductionList());

        [HttpGet("production/detail/{id:int}")]
        public async Task<ActionResult<Production>> GetDetail(int id)
            => Ok(await _productionService.GetProductionDetail(id));

        [HttpPatch("revision-request/{id:int}")]
        public async Task<ActionResult<Production>> RequestRevision(int id)
            => Ok(await _productionService.RequestProductionRevision(id));

        [HttpPatch("production/deny/{id:int}")]
        public async Task<ActionResult<Production>> Deny(int id, [FromBody] RejectProductionDTO dto)
            => Ok(await _productionService.DenyProduction(id, dto.UserId, dto.Reason));

        [HttpPut("production/update/{id:int}")]
        public async Task<ActionResult<Production>> Update(int id, [FromBody] UpdateProductionDTO dto)
            => Ok(await _productionService.UpdateProduction(id, new Production
            {
                PmId = dto.PmId,
                OrderId = dto.OrderId,
                //  StatusId = dto.StatusId
            }));

        [HttpGet("production-plans/pending")]
        public async Task<ActionResult<IEnumerable<Production>>> GetPendingPlans()
            => Ok(await _productionService.GetPendingProductionPlans());

        [HttpGet("production-plans")]
        public async Task<ActionResult<IEnumerable<Production>>> GetPlanList()
            => Ok(await _productionService.GetProductionPlanList());

        [HttpPost("production-plans/config/{id:int}")]
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

        [HttpGet("production-plans/detail/{id:int}")]
        public async Task<ActionResult<Production>> GetPlanDetail(int id)
            => Ok(await _productionService.GetProductionPlanDetail(id));

        [HttpPatch("production-plans/deny/{id:int}")]
        public async Task<ActionResult<Production>> DenyPlan(int id, [FromBody] RejectProductionDTO dto)
            => Ok(await _productionService.DenyProductionPlan(id, dto.UserId, dto.Reason));
    }
}
