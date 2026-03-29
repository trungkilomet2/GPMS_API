using AutoMapper;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Common;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuttingNotebookController : ControllerBase
    {
        private readonly ICuttingNotebookRepositories _service;
        private readonly IMapper _mapper;
        public CuttingNotebookController(ICuttingNotebookRepositories service,IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpPost("notebook/create")]
        public async Task<ActionResult<RestDTO<CuttingNotebook>>> Create([FromBody] CreateCuttingNotebookDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                var data = await _service.CreateNotebook(new CuttingNotebook
                {
                    ProductionId = dto.ProductionId,
                    MarkerLength = dto.MarkerLength,
                    FabricWidth = dto.FabricWidth
                });
                return StatusCode(StatusCodes.Status201Created, new RestDTO<CuttingNotebook> { Data = data });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails { Detail = ex.Message, Status = 400 });
            }
        }

        [HttpGet("notebook/production/{productionId:int}")]
        public async Task<ActionResult<RestDTO<IEnumerable<CuttingNotebook>>>> ListByProduction([Range(1, int.MaxValue)] int productionId)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            var data = await _service.GetByProduction(productionId);
            return Ok(new RestDTO<IEnumerable<CuttingNotebook>> { Data = data });
        }

        [HttpGet("notebook/{notebookId:int}")]
        public async Task<ActionResult<RestDTO<CuttingNotebook>>> Detail([Range(1, int.MaxValue)] int notebookId)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            var data = await _service.GetNotebook(notebookId);
            return Ok(new RestDTO<CuttingNotebook> { Data = data });
        }

        [HttpPost("notebook/create-logs/{notebookId:int}")]
        public async Task<ActionResult<RestDTO<CuttingNotebookLog>>> CreateLog([Range(1, int.MaxValue)] int notebookId, [FromBody] CreateCuttingNotebookLogDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                var data = await _service.CreateLog(new CuttingNotebookLog
                {
                    NotebookId = notebookId,
                    UserId = dto.UserId,
                    Color = dto.Color,
                    MeterPerKg = dto.MeterPerKg,
                    Layer = dto.Layer,
                    ProductQty = dto.ProductQty,
                    AvgConsumption = dto.AvgConsumption,
                    Note = dto.Note,
                    DateCreate = DateOnly.FromDateTime(VietnamTime.Now()),
                    IsPayment = false,
                    IsReadOnly = false
                });
                return StatusCode(StatusCodes.Status201Created, new RestDTO<CuttingNotebookLog> { Data = data });
            }
            catch (ValidationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetails { Detail = ex.Message, Status = 400 });
            }
        }

        [HttpGet("notebook/get-list-logs/{notebookId:int}")]
        public async Task<ActionResult<RestDTO<IEnumerable<CuttingNotebookLog>>>> ListLogs([Range(1, int.MaxValue)] int notebookId)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            var data = await _service.GetLogs(notebookId);
            return Ok(new RestDTO<IEnumerable<CuttingNotebookLog>> { Data = data });
        }

        // HTTP PUT : Cập Nhật SỔ CẮT
        [HttpPut("notebook/update/{notebookId:int}")]
        public async Task<ActionResult<RestDTO<CuttingNotebookResponseDTO>>> UpdateNotebook([Range(1, int.MaxValue)] int notebookId, [FromBody] UpdateCuttingNotebookDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                var data = await _service.UpdateNotebook(new CuttingNotebook
                {
                    Id = notebookId,
                    MarkerLength = dto.MarkerLength,
                    FabricWidth = dto.FabricWidth
                });
                return Ok(new RestDTO<CuttingNotebookResponseDTO> { Data = _mapper.Map<CuttingNotebookResponseDTO>(data) });
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

        // HTTP PUT : Cập Nhật Log của sổ cắt
        [HttpPut("notebook/log/update/{logId:int}")]
        public async Task<ActionResult<RestDTO<CuttingNotebookLogResponseDTO>>> UpdateLog([Range(1, int.MaxValue)] int logId, [FromBody] UpdateCuttingNotebookLogDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                var data = await _service.UpdateLog(new CuttingNotebookLog
                {
                    Id = logId,
                    Color = dto.Color,
                    MeterPerKg = dto.MeterPerKg,
                    Layer = dto.Layer,
                    ProductQty = dto.ProductQty,
                    AvgConsumption = dto.AvgConsumption,
                    Note = dto.Note
                });
                return Ok(new RestDTO<CuttingNotebookLogResponseDTO> { Data = _mapper.Map<CuttingNotebookLogResponseDTO>(data) });
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

        // HTTP DELETE: cập nhật xóa của sổ cắt
        [HttpDelete("notebook/log/delete/{logId:int}")]
        public async Task<ActionResult> DeleteLog([Range(1, int.MaxValue)] int logId)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));
            try
            {
                await _service.DeleteLog(logId);
                return Ok();
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