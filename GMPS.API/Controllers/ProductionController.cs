using AutoMapper;
using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.WebSockets;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GMPS.API.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly IProductionRepositories _productionService;
        private readonly IMapper _mapper;
        private readonly ILogger<Production> _logger;

        public ProductionController(IProductionRepositories productionService, ILogger<Production> logger, IMapper mapper)
        {
            _mapper = mapper;
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
                  //  var production_detail_view = await _productionService.GetProductionDetailView(result.Id);
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
                StatusCodes.Status400BadRequest,
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
        public async Task<ActionResult<IEnumerable<ListProductionDTO>>> GetList([FromQuery] RequestDTO<Production> input)
        {
            // Lấy danh sách theo input từ csdl
            var data = await _productionService.GetProductionListViews();

            if (data.Count() == 0)
            {
                return NoContent();
            }
            //filter data
            var result = data.Skip(input.PageIndex * input.PageSize)
                        .Take(input.PageSize);
            _logger.LogInformation(CustomLogEvents.ProductionController_Get,$" Đã lấy được ({data.Count()}) Production");
            return Ok(new RestDTO<IEnumerable<ListProductionDTO>>
            {
                Data = _mapper.Map<IEnumerable<ListProductionDTO>>(result),
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = data.Count(),
                Links = new List<LinkDTO>
                {
             //      new LinkDTO(Url.Action(null,"production/list",input,Request.Scheme!),"self","GET")
                }
            });
        }

        //Get: Lấy thông tin chi tiết của production đấy

        [HttpGet("production/detail/{production_id:int}")]
        public async Task<ActionResult<RestDTO<ProductionDetailDTO>>> GetDetail([Required] int production_id)
        {
            if (production_id < 0)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = "Production ID Nhập vào phải là một số dương";
                _logger.LogError(CustomLogEvents.ProductionController_Get, $"Lấy Thông Tin Chi Tiết Không Thành Công Production ID = ({production_id})");
                exceptionDetails.Status =
                StatusCodes.Status400BadRequest;
                return StatusCode(
                StatusCodes.Status400BadRequest,
                exceptionDetails);
            }
            try
            {
                var data = await _productionService.GetProductionDetail(production_id);
                if (data is null) NoContent();
                _logger.LogInformation(CustomLogEvents.ProductionController_Get, $"Lấy thành công ProductionId = {production_id})");
                return Ok(new RestDTO<ProductionDetailDTO>
                {
                    Data = _mapper.Map<ProductionDetailDTO>(data),
                    Links = new List<LinkDTO>
                        {
                    //        new LinkDTO(Url.Action(null,$"production/detail/{production_id}",data,Request.Scheme!),"self","POST")
                        }
                });
            }
            catch (ValidationException ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status =
                StatusCodes.Status400BadRequest;
                return StatusCode(
                StatusCodes.Status400BadRequest,
                exceptionDetails);
            }
        }
        #region TRUNG - Revision Request Production Plan Đang Chờ Xét Duyệt - Chưa Sử Dụng API

        // HttpPatch : Yêu Cầu Người dùng Cập nhật Lại đơn hàng
        
        //[HttpPatch("production/revision-request/{id:int}")]
        //public async Task<ActionResult<Production>> RequestRevision(int production_id)
        //{
        //    if (production_id < 0)
        //    {
        //        var exceptionDetails = new ProblemDetails();
        //        exceptionDetails.Detail = "Production ID Nhập vào phải là một số dương";
        //        exceptionDetails.Status =
        //        StatusCodes.Status400BadRequest;
        //        return StatusCode(
        //        StatusCodes.Status400BadRequest,
        //        exceptionDetails);
        //    }
        //    try
        //    {
        //        var data = await _productionService.RequestProductionRevision(production_id);
        //        if (data is null) NoContent();
        //        _logger.LogInformation(CustomLogEvents.ProductionController_Put, $" Cập Nhật Thành Công Production ID = ({data.Id}) thành trạng thái ({ProductionStatus_Constants.NeedUpdate})");
        //        return Ok(new RestDTO<ProductionDetailDTO>
        //        {
        //            Data = _mapper.Map<ProductionDetailDTO>(data),
        //            Links = new List<LinkDTO>
        //                {
        //               //     new LinkDTO(Url.Action(null,$"production/detail/{production_id}",data,Request.Scheme!),"self","POST")
        //                }
        //        });
        //    }
        //    catch (DBConcurrencyException ex)
        //    {
        //        var exceptionDetails = new ProblemDetails();
        //        exceptionDetails.Detail = ex.Message;
        //        exceptionDetails.Status =
        //        StatusCodes.Status500InternalServerError;
        //        return StatusCode(
        //        StatusCodes.Status500InternalServerError,
        //        exceptionDetails);
        //    }
        //    catch (ValidationException ex)
        //    {
        //        var exceptionDetails = new ProblemDetails();
        //        exceptionDetails.Detail = ex.Message;
        //        exceptionDetails.Status =
        //        StatusCodes.Status400BadRequest;
        //        return StatusCode(
        //        StatusCodes.Status400BadRequest,
        //        exceptionDetails);
        //    }
        //}

        #endregion


        #region TRUNG - Deny Production Plan Đang Chờ Xét Duyệt - Chưa Sử Dụng API
        //[HttpPatch("production/deny/{id:int}")]
        //public async Task<ActionResult<Production>> Deny(int id, [FromBody] RejectProductionDTO dto)
        //{
        //    var exceptionDetails = new ProblemDetails();
        //    exceptionDetails.Detail = "Đang Xem Sett Tính Lăng Lày";
        //    exceptionDetails.Status =
        //    StatusCodes.Status400BadRequest;
        //    return StatusCode(
        //    StatusCodes.Status400BadRequest,
        //    exceptionDetails);
        //}
        #endregion

        [HttpPut("production/update-pm/{production_id:int}/{new_pm_id:int}")]
        public async Task<ActionResult<Production>> Update(int production_id, int new_pm_id)
        {
            if (production_id < 0)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = "Production ID Nhập vào phải là một số dương - Code : 400";
                exceptionDetails.Status =
                StatusCodes.Status400BadRequest;
                return StatusCode(
                StatusCodes.Status400BadRequest,
                exceptionDetails);
            }
            if (new_pm_id < 0)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = "PM ID Nhập vào phải là một số dương - Code : 400";
                exceptionDetails.Status =
                StatusCodes.Status400BadRequest;
                return StatusCode(
                StatusCodes.Status400BadRequest,
                exceptionDetails);
            }
            try
            {
                var data = await _productionService.UpdatePMProduction(production_id,new_pm_id);
                if (data is null) NoContent();
                _logger.LogInformation(CustomLogEvents.ProductionController_Put, $"Cập Nhật Thành Công Production ID = ({data.Id}) thành trạng thái  - Code : 200");
                return Ok(new RestDTO<Production>
                {
                    Data = data,
                    Links = new List<LinkDTO>
                        {
                      //      new LinkDTO(Url.Action(null,$"production/detail/{production_id}",data,Request.Scheme!),"self","POST")
                        }
                });
            }
            catch (DBConcurrencyException ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status =
                StatusCodes.Status500InternalServerError;
                _logger.LogError(CustomLogEvents.ProductionController_Put, "Lỗi xảy ra ở hệ thống - Code : 500");
                return StatusCode(
                StatusCodes.Status500InternalServerError,
                exceptionDetails);
            }
            catch (ValidationException ex)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status =
                StatusCodes.Status400BadRequest;
                _logger.LogError(CustomLogEvents.ProductionController_Put, "Lỗi ở trường nhập liệu - Code : 400");
                return StatusCode(
                StatusCodes.Status400BadRequest,
                exceptionDetails);
            }
        }


        #region Filter Production Plan Đang Ở Trạng Thái Sản Xuất - Chưa Sử Dung API
        // HTTP GET: Xem danh sách Production Plan đang chờ được xem xét
        // Actor : Chủ Xưởng - Admin : Xem danh sách các Production Plan đang ở trạng thái chờ xét duyệt

        //[HttpGet("production-plans/pending")]
        //public async Task<ActionResult<IEnumerable<Production>>> GetPendingPlans(RequestDTO<Production> input)
        //{
        //    // Lấy danh sách theo input từ csdl
        //    var data = await _productionService.GetPendingProductionPlans();
        //    if (data.Count() == 0)
        //    {
        //        return NoContent();
        //    }
        //    //filter data
        //    var result = data.Skip(input.PageIndex * input.PageSize)
        //                .Take(input.PageSize);
        //    return Ok(new RestDTO<IEnumerable<ListProductionDTO>>
        //    {
        //        Data = _mapper.Map<IEnumerable<ListProductionDTO>>(result),
        //        PageIndex = input.PageIndex,
        //        PageSize = input.PageSize,
        //        RecordCount = data.Count(),
        //        Links = new List<LinkDTO>
        //        {
        //            //      new LinkDTO(Url.Action(null,"production/list",input,Request.Scheme!),"self","GET")
        //        }
        //    });
        //    throw new Exception("hmmmmmm");   
        //}

        #endregion

        //------------------------- PRODUCTION STATUS API------------------------------- CHECK2

        [HttpPatch("production/approve/{production_id:int}")]
        public async Task<ActionResult<RestDTO<ProductionDetailDTO>>> ApproveProduction(
          [Range(1, int.MaxValue)] int production_id
          )
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                await _productionService.ApproveProduction(production_id);
                var data = await _productionService.GetProductionDetail(production_id);
                return Ok(new RestDTO<ProductionDetailDTO> { Data = _mapper.Map<ProductionDetailDTO>(data) });
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

        [HttpPatch("production/reject/{production_id:int}")]
        public async Task<ActionResult<RestDTO<ProductionDetailDTO>>> RejectProduction(
            [Range(1, int.MaxValue)] int production_id,
            [FromBody] RejectProductionRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                await _productionService.RejectProduction(production_id, dto.Reason);
                var data = await _productionService.GetProductionDetail(production_id);
                return Ok(new RestDTO<ProductionDetailDTO> { Data = _mapper.Map<ProductionDetailDTO>(data) });
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

        [HttpGet("production/issues/{production_id:int}")]
        public async Task<ActionResult<RestDTO<IEnumerable<ProductionIssueListItemDTO>>>> GetIssues([Range(1, int.MaxValue)] int production_id)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                var issues = await _productionService.GetProductionIssues(production_id);
                var data = issues.Select(x => new ProductionIssueListItemDTO
                {
                    IssueId = x.Id,
                    TypeIssue = x.TypeIssue,
                    Title = x.Title,
                    Description = x.Description,
                    Priority = x.Priority,
                    Quantity = 1,
                    ImageUrl = x.ImageUrl,
                    CreatedAt = x.CreatedAt
                });
                return Ok(new RestDTO<IEnumerable<ProductionIssueListItemDTO>> { Data = data });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails { Detail = ex.Message, Status = 400 });
            }
        }

        [HttpGet("production/issues/summary-by-type/{production_id:int}")]
        public async Task<ActionResult<RestDTO<IEnumerable<ProductionIssueSummaryDTO>>>> GetIssueSummaryByType([Range(1, int.MaxValue)] int production_id)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                var summary = await _productionService.GetProductionIssueSummaryByType(production_id);
                var data = summary.Select(x => new ProductionIssueSummaryDTO
                {
                    TypeIssue = x.TypeIssue,
                    TotalIssues = x.Id,
                    TotalQuantity = x.Id,
                    LastIssueAt = x.CreatedAt
                });
                return Ok(new RestDTO<IEnumerable<ProductionIssueSummaryDTO>> { Data = data });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails { Detail = ex.Message, Status = 400 });
            }
        }


    }
}
