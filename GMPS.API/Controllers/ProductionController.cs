using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.WebSockets;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly IProductionRepositories _productionService;
        private readonly ILogger<Production> _logger;

        public ProductionController(IProductionRepositories productionService, ILogger<Production> logger)
        {
            _productionService = productionService;
            _logger = logger;
        }

        [HttpPost("production/create")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<RestDTO<Production>>> CreateProduction([FromBody] CreateProductionDTO dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Tạo đơn hàng với trạng thái mặc định là "Chờ Xét Duyệt"s
                    var result = await _productionService.CreateProduction(new Production
                    {
                        PmId = dto.PmId,
                        OrderId = dto.OrderId,
                        StatusId = ProductionStatus_Constants.Pending_ID
                    });
                    _logger.LogInformation(CustomLogEvents.ProductionController_Get, $"Create successfully ProductionId = ({result.Id})");
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
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch (ValidationException ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status =
                StatusCodes.Status400BadRequest;
                return StatusCode(
                StatusCodes.Status401Unauthorized,
                exceptionDetails);
            }
            catch (Exception ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status =
                StatusCodes.Status500InternalServerError;
                return StatusCode(
                StatusCodes.Status500InternalServerError,
                exceptionDetails);
            }
        }

        [HttpGet("production/list")]
      //  [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<IEnumerable<Production>>> GetList([FromQuery] RequestDTO<Production> input)
        {
            // Lấy danh sách theo input từ csdl
            var data = await _productionService.GetProductionList();
            //filter data
            var result = data.Skip(input.PageIndex * input.PageSize)
                        .Take(input.PageSize).ToList();

            return Ok(new RestDTO<IEnumerable<Production>>
            {
                Data = result,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = data.Count(),
                Links = new List<LinkDTO>
                {
                   new LinkDTO(Url.Action(null,"production/list",input,Request.Scheme!),"self","GET")
                }
            });
        }

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
