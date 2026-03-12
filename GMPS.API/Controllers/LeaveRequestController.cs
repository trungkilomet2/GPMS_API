using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestRepositories _leaveRequestRepo;
        private readonly ILogger<LeaveRequestController> _logger;

        public LeaveRequestController(ILeaveRequestRepositories leaveRequestRepo, ILogger<LeaveRequestController> logger)
        {
            _leaveRequestRepo = leaveRequestRepo ?? throw new ArgumentNullException(nameof(leaveRequestRepo));
            _logger = logger;
        }

        // GET api/leaverequest/leave-request-list
        [HttpGet("leave-request-list", Name = "Get all leave request list")]
        [Authorize(Roles = "Owner,PM")]
        public async Task<ActionResult<RestDTO<IEnumerable<LeaveRequestListDTO>>>> GetLeaveRequests([FromQuery] RequestDTO<LeaveRequest> input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Get,
                    "Getting all leave requests - PageIndex: {PageIndex}, PageSize: {PageSize}",
                    input.PageIndex, input.PageSize);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "Invalid model state while getting all leave requests");

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var result = await _leaveRequestRepo.GetAllLeaveRequests();

                if (!string.IsNullOrEmpty(input.FilterQuery))
                    result = result.Where(lr => lr.UserFullName != null &&
                        lr.UserFullName.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase));

                var recordCount = result.Count();
                var totalPages = (int)Math.Ceiling((double)recordCount / input.PageSize);

                if (recordCount > 0 && input.PageIndex >= totalPages)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "PageIndex {PageIndex} out of range. Total pages: {TotalPages}",
                        input.PageIndex, totalPages);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "pageIndex", new[] { $"Page {input.PageIndex} not exist. Total number of pages currently available: {totalPages}" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                var data = result
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize)
                    .Select(lr => new LeaveRequestListDTO
                    {
                        Id = lr.Id,
                        UserId = lr.UserId,
                        UserFullName = lr.UserFullName,
                        Content = lr.Content,
                        DateCreate = lr.DateCreate,
                        DateReply = lr.DateReply,
                        DenyContent = lr.DenyContent,
                        Status = lr.StatusName
                    });

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Get,
                    "Returned {Count} leave requests successfully", data.Count());

                return Ok(new RestDTO<IEnumerable<LeaveRequestListDTO>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "LeaveRequest", new { input.PageIndex, input.PageSize }, Request.Scheme)!, "self", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.LeaveRequestController_Get, ex,
                    "Error occurred while getting all leave requests");

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }
    }
}