using AutoMapper;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


//- Lấy tất cả production part của Production đấy
//- Xem thông tin chi tiết Assign Của Production Part Đấy
//- Thêm danh sách Production Part vào một Production
//- Cập nhật thông tin của một Production Part (ví dụ: thay đổi số lượng, trạng thái, v.v.)
//- Phân công Production Part cho một nhóm Worker
// Xóa một producion part trong một production (nếu có thể, tùy vào nghiệp vụ hệ thống có cho phép hay không)


namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionPartController : ControllerBase
    {
        private readonly IProductionPartRepositories _productionPartService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductionPartController> _logger;

        public ProductionPartController(
            IProductionPartRepositories productionPartService,
            IMapper mapper,
            ILogger<ProductionPartController> logger)
        {
            _productionPartService = productionPartService;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: Lấy tất cả Production Part của Production ID đấy
        [HttpGet("production/get-list-parts/{productionId:int}")]
        public async Task<ActionResult<RestDTO<IEnumerable<ProductionPartDetailDTO>>>> GetParts(
            [Range(1, int.MaxValue)] int productionId,
            [FromQuery] RequestDTO<ProductionPart> input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            try
            {
                var data = (await _productionPartService.GetPartsByProductionId(productionId)).ToList();
                if (!data.Any())
                {
                    return NoContent();
                }

                var paged = data.Skip(input.PageIndex * input.PageSize).Take(input.PageSize);

                return Ok(new RestDTO<IEnumerable<ProductionPartDetailDTO>>
                {
                    Data = _mapper.Map<IEnumerable<ProductionPartDetailDTO>>(paged),
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = data.Count
                });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get production parts for production {ProductionId}", productionId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // GET: Xem thông tin chi tiết Assign Của Production Part Đấy, bao gồm cả thông tin về các worker đã được phân công cho phần đó
        [HttpGet("parts/assign-detail/{partId:int}")]
        public async Task<ActionResult<RestDTO<ProductionPartDetailDTO>>> GetAssignDetail([Range(1, int.MaxValue)] int partId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var data = await _productionPartService.GetPartAssignmentDetail(partId);
                return Ok(new RestDTO<ProductionPartDetailDTO>
                {
                    Data = _mapper.Map<ProductionPartDetailDTO>(data)
                });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assignment detail for part {PartId}", partId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // GET : Tạo ra part của production đấy, có thể tạo nhiều part cùng lúc, trả về thông tin chi tiết của các part vừa được tạo ra
        // Truyền vào một IEnumerable danh sách cùng một lúc
        [HttpPost("production/create-parts/{productionId:int}")]
        public async Task<ActionResult<RestDTO<IEnumerable<ProductionPartDetailDTO>>>> CreateParts(
            [Range(1, int.MaxValue)] int productionId,
            [FromBody] CreateProductionPartListDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var parts = dto.Parts.Select(x => new ProductionPart
                {
                    ProductionId = productionId,
                    PartName = x.PartName,
                    TeamLeaderId = x.TeamLeaderId,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Cpu = x.Cpu,
                    StatusId = ProductionPart_Constrants.ToDo_ID
                });

                var data = await _productionPartService.CreateParts(productionId, parts);

                return StatusCode(StatusCodes.Status201Created, new RestDTO<IEnumerable<ProductionPartDetailDTO>>
                {
                    Data = _mapper.Map<IEnumerable<ProductionPartDetailDTO>>(data)
                });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create production parts for production {ProductionId}", productionId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // PUT: Cập nhật thông tin của MỘT Production Part (ví dụ: thay đổi số lượng, trạng thái, v.v.)
        [HttpPut("parts/update/{partId:int}")]
        public async Task<ActionResult<RestDTO<ProductionPartDetailDTO>>> UpdatePart(
            [Range(1, int.MaxValue)] int partId,
            [FromBody] UpdateProductionPartDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var part = new ProductionPart
                {
                    PartName = dto.PartName,
                    TeamLeaderId = dto.TeamLeaderId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Cpu = dto.Cpu,
                    StatusId = dto.StatusId
                };
                var data = await _productionPartService.UpdatePart(partId, part);
                return Ok(new RestDTO<ProductionPartDetailDTO>
                {
                    Data = _mapper.Map<ProductionPartDetailDTO>(data)
                });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update part {PartId}", partId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // PATCH: Phân công Production Part cho một nhóm Worker,
        // truyền vào một list workerId cùng một lúc,
        // trả về thông tin chi tiết của part sau khi đã được phân công
        [HttpPatch("parts/update-assign-workers/{partId:int}")]
        public async Task<ActionResult<RestDTO<ProductionPartDetailDTO>>> AssignWorkers(
                    [Range(1, int.MaxValue)] int partId,
                    [FromBody] AssignProductionPartWorkersDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var data = await _productionPartService.AssignWorkers(partId, dto.WorkerIds);
                return Ok(new RestDTO<ProductionPartDetailDTO>
                {
                    Data = _mapper.Map<ProductionPartDetailDTO>(data)
                });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign workers to part {PartId}", partId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
        //GET: Xóa một producion part trong một production (nếu có thể, tùy vào nghiệp vụ hệ thống có cho phép hay không)
        [HttpDelete("parts/delete/{partId:int}")]
        public async Task<ActionResult> DeletePart([Range(1, int.MaxValue)] int partId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                await _productionPartService.DeletePart(partId);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete part {PartId}", partId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}