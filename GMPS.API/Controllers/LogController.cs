using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        public readonly ILogEventRepositories _logEventRepo;
        private readonly ILogger<LogController> _logger;

        public LogController(ILogEventRepositories logEventRepo, ILogger<LogController> logger)
        {
            _logEventRepo = logEventRepo ?? throw new ArgumentNullException(nameof(logEventRepo));
            _logger = logger;
        }

        [HttpGet("get-all-log-events")]
        [Authorize]
        public async Task<ActionResult<RestDTO<IEnumerable<LogEventDTO>>>> GetLogs([FromQuery] RequestDTO<LogEventDTO> input, [FromQuery] DateTime? fromTimestamp,
    [FromQuery] DateTime? toTimestamp)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.LogController_Get,
                    "Getting all log events - PageIndex: {PageIndex}, PageSize: {PageSize}, SortColumn: {SortColumn}, SortOrder: {SortOrder}, FilterQuery: {FilterQuery} ",
                    input.PageIndex, input.PageSize, input.SortColumn, input.SortOrder, input.FilterQuery);

                if (!ModelState.IsValid)
                {
                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest
                    };
                    return BadRequest(errorDetails);
                }

                var result = await _logEventRepo.GetAllLog();

                if (!string.IsNullOrEmpty(input.FilterQuery?.Trim()))
                {
                    result = result.Where(u =>
                        (u.Message != null && u.Message.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.MessageTemplate != null && u.MessageTemplate.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.Level != null && u.Level.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.Exception != null && u.Exception.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (u.Properties != null && u.Properties.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase))
                    );
                }

                if (fromTimestamp.HasValue)
                {
                    result = result.Where(u => u.TimeStemp.HasValue && u.TimeStemp.Value >= fromTimestamp.Value);
                }

                if (toTimestamp.HasValue)
                {
                    result = result.Where(u => u.TimeStemp.HasValue && u.TimeStemp.Value <= toTimestamp.Value);
                }

                var recordCount = result.Count();
                var data = result
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize)
                    .Select(u => new LogEventDTO
                    {
                        Id = u.Id,
                        Message = u.Message,
                        MessageTemplate = u.MessageTemplate,
                        Level = u.Level,
                        TimeStemp = u.TimeStemp,
                        Exception = u.Exception,
                        Properties = u.Properties
                    })
                    .ToList();

                _logger.LogInformation(CustomLogEvents.LogController_Get,
                    "Returned {Count} log events", data.Count);
                return StatusCode(StatusCodes.Status200OK,new RestDTO<IEnumerable<LogEventDTO>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Links = new List<LinkDTO>
    {
        new LinkDTO(
            Url.Action(null, "Log",
                new { input.PageIndex, input.PageSize },
                Request.Scheme)!,
            "self",
            "GET"
        )
    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.Error_Get, ex,
                    "Error while getting log events");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }
    }
}
