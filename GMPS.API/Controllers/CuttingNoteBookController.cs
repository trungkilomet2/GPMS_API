using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuttingNotebookController : ControllerBase
    {
        private readonly ICuttingNotebookRepositories _service;

        public CuttingNotebookController(ICuttingNotebookRepositories service)
        {
            _service = service;
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
                    DateCreate = DateOnly.FromDateTime(DateTime.UtcNow),
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
    }
}