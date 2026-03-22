using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
        public async Task<ActionResult<RestDTO<IEnumerable<LeaveRequestListDTO>>>> GetLeaveRequests([FromQuery] LeaveRequestRequestDTO input)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Get,
                    "Getting all leave requests - PageIndex: {PageIndex}, PageSize: {PageSize}, Status: {Status}, DateCreateFrom: {DateCreateFrom}, DateCreateTo: {DateCreateTo}",
                    input.PageIndex, input.PageSize, input.Status, input.DateCreateFrom, input.DateCreateTo);

                if (!string.IsNullOrEmpty(input.Status) &&
                    input.Status != LeaveRequestStatus_Constants.Pending &&
                    input.Status != LeaveRequestStatus_Constants.Approved &&
                    input.Status != LeaveRequestStatus_Constants.Denied)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "Invalid Status value '{Status}' provided", input.Status);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "status", new[] { $"Status must be one of: '{LeaveRequestStatus_Constants.Pending}', '{LeaveRequestStatus_Constants.Approved}', '{LeaveRequestStatus_Constants.Denied}'." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (input.DateCreateFrom.HasValue && input.DateCreateTo.HasValue
                    && input.DateCreateFrom > input.DateCreateTo)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "DateCreateFrom {DateCreateFrom} is greater than DateCreateTo {DateCreateTo}",
                        input.DateCreateFrom, input.DateCreateTo);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "dateCreateFrom", new[] { "DateCreateFrom must be less than or equal to DateCreateTo." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

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

                if (!string.IsNullOrEmpty(input.Status))
                    result = result.Where(lr => lr.StatusName != null &&
                        lr.StatusName.Equals(input.Status, StringComparison.OrdinalIgnoreCase));

                if (input.DateCreateFrom.HasValue)
                    result = result.Where(lr => lr.DateCreate >= input.DateCreateFrom.Value.Date);

                if (input.DateCreateTo.HasValue)
                    result = result.Where(lr => lr.DateCreate < input.DateCreateTo.Value.Date.AddDays(1));

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
                        FromDate = lr.FromDate,
                        ToDate = lr.ToDate,
                        DateReply = lr.DateReply,
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

        // GET api/leaverequest/my-leave-request-history
        [HttpGet("my-leave-request-history", Name = "Get my leave request history")]
        [Authorize(Roles = "Owner,PM,Team_Leader,Worker")]
        public async Task<ActionResult<RestDTO<IEnumerable<LeaveRequestListDTO>>>> GetMyLeaveRequestHistory([FromQuery] LeaveRequestRequestDTO input)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim is null || !int.TryParse(userIdClaim, out var requesterId))
                    return Unauthorized();

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Get,
                    "Getting leave request history for UserId {UserId} - PageIndex: {PageIndex}, PageSize: {PageSize}, Status: {Status}, DateCreateFrom: {DateCreateFrom}, DateCreateTo: {DateCreateTo}",
                    requesterId, input.PageIndex, input.PageSize, input.Status, input.DateCreateFrom, input.DateCreateTo);

                if (!string.IsNullOrEmpty(input.Status) &&
                    input.Status != LeaveRequestStatus_Constants.Pending &&
                    input.Status != LeaveRequestStatus_Constants.Approved &&
                    input.Status != LeaveRequestStatus_Constants.Denied)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "UserId {UserId} provided invalid Status value '{Status}'", requesterId, input.Status);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "status", new[] { $"Status must be one of: '{LeaveRequestStatus_Constants.Pending}', '{LeaveRequestStatus_Constants.Approved}', '{LeaveRequestStatus_Constants.Denied}'." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (input.DateCreateFrom.HasValue && input.DateCreateTo.HasValue
                    && input.DateCreateFrom > input.DateCreateTo)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "UserId {UserId}: DateCreateFrom {DateCreateFrom} is greater than DateCreateTo {DateCreateTo}",
                        requesterId, input.DateCreateFrom, input.DateCreateTo);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "dateCreateFrom", new[] { "DateCreateFrom must be less than or equal to DateCreateTo." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "Invalid model state while getting leave request history for UserId {UserId}", requesterId);

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

                var result = await _leaveRequestRepo.GetLeaveRequestsByUserId(requesterId);

                if (!string.IsNullOrEmpty(input.Status))
                    result = result.Where(lr => lr.StatusName != null &&
                        lr.StatusName.Equals(input.Status, StringComparison.OrdinalIgnoreCase));

                if (input.DateCreateFrom.HasValue)
                    result = result.Where(lr => lr.DateCreate >= input.DateCreateFrom.Value.Date);

                if (input.DateCreateTo.HasValue)
                    result = result.Where(lr => lr.DateCreate < input.DateCreateTo.Value.Date.AddDays(1));

                var recordCount = result.Count();
                var totalPages = (int)Math.Ceiling((double)recordCount / input.PageSize);

                if (recordCount > 0 && input.PageIndex >= totalPages)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "UserId {UserId}: PageIndex {PageIndex} out of range. Total pages: {TotalPages}",
                        requesterId, input.PageIndex, totalPages);

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
                        FromDate = lr.FromDate,
                        ToDate = lr.ToDate,
                        DateReply = lr.DateReply,
                        Status = lr.StatusName
                    });

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Get,
                    "Returned {Count} leave request history records for UserId {UserId} successfully", data.Count(), requesterId);

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
                    "Error occurred while getting leave request history for current user");

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // GET api/leaverequest/my-leave-request-history/{id}
        [HttpGet("my-leave-request-history/{id}", Name = "Get my leave request history detail")]
        [Authorize(Roles = "Owner,PM,Team_Leader,Worker")]
        public async Task<ActionResult<RestDTO<LeaveRequestDetailDTO>>> GetMyLeaveRequestHistoryDetail(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim is null || !int.TryParse(userIdClaim, out var requesterId))
                    return Unauthorized();

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Get,
                    "UserId {UserId} getting their leave request history detail for LeaveRequestId {LeaveRequestId}",
                    requesterId, id);

                if (id <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "UserId {UserId} provided invalid LeaveRequestId {LeaveRequestId} - must be greater than 0",
                        requesterId, id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Leave request Id must be greater than 0." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var leaveRequest = await _leaveRequestRepo.GetLeaveRequestById(id);

                if (leaveRequest is null)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "LeaveRequestId {LeaveRequestId} not found for UserId {UserId}", id, requesterId);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Leave request with id '{id}' not found." } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                if (leaveRequest.UserId != requesterId)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "UserId {RequesterId} attempted to access LeaveRequestId {LeaveRequestId} belonging to UserId {OwnerId}",
                        requesterId, id, leaveRequest.UserId);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "authorization", new[] { "You do not have permission to view this leave request." } }
                    };
                    return StatusCode(StatusCodes.Status403Forbidden, errorDetails);
                }

                var data = new LeaveRequestDetailDTO
                {
                    Id = leaveRequest.Id,
                    UserId = leaveRequest.UserId,
                    UserFullName = leaveRequest.UserFullName,
                    Content = leaveRequest.Content,
                    DateCreate = leaveRequest.DateCreate,
                    FromDate = leaveRequest.FromDate,
                    ToDate = leaveRequest.ToDate,
                    DateReply = leaveRequest.DateReply,
                    DenyContent = leaveRequest.DenyContent,
                    ApprovedByName = leaveRequest.ApprovedByName,
                    Status = leaveRequest.StatusName
                };

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Get,
                    "Returned leave request history detail for LeaveRequestId {LeaveRequestId} to UserId {UserId} successfully",
                    id, requesterId);

                return Ok(new RestDTO<LeaveRequestDetailDTO>
                {
                    Data = data,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action("GetMyLeaveRequestHistoryDetail", "LeaveRequest", new { id }, Request.Scheme)!, "self", "GET"),
                        new LinkDTO(Url.Action("GetMyLeaveRequestHistory", "LeaveRequest", null, Request.Scheme)!, "my-leave-request-history", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.LeaveRequestController_Get, ex,
                    "Error occurred while getting leave request history detail for LeaveRequestId {LeaveRequestId}", id);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // GET api/leaverequest/leave-request-detail/{id}
        [HttpGet("leave-request-detail/{id}", Name = "Get leave request detail by id")]
        [Authorize(Roles = "Owner,PM")]
        public async Task<ActionResult<RestDTO<LeaveRequestDetailDTO>>> GetLeaveRequestDetail(int id)
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Get,
                    "Getting leave request detail for LeaveRequestId {LeaveRequestId}", id);

                if (id <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "Invalid LeaveRequestId {LeaveRequestId} - must be greater than 0", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Leave request Id must be greater than 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                var leaveRequest = await _leaveRequestRepo.GetLeaveRequestById(id);

                if (leaveRequest is null)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Get,
                        "Leave request {LeaveRequestId} not found", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { $"Leave request with id '{id}' not found" } }
                    };
                    return StatusCode(StatusCodes.Status404NotFound, errorDetails);
                }

                var data = new LeaveRequestDetailDTO
                {
                    Id = leaveRequest.Id,
                    UserId = leaveRequest.UserId,
                    UserFullName = leaveRequest.UserFullName,
                    Content = leaveRequest.Content,
                    DateCreate = leaveRequest.DateCreate,
                    FromDate = leaveRequest.FromDate,
                    ToDate = leaveRequest.ToDate,
                    DateReply = leaveRequest.DateReply,
                    DenyContent = leaveRequest.DenyContent,
                    ApprovedByName = leaveRequest.ApprovedByName,
                    Status = leaveRequest.StatusName
                };

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Get,
                    "Returned detail for LeaveRequestId {LeaveRequestId} successfully", id);

                return Ok(new RestDTO<LeaveRequestDetailDTO>
                {
                    Data = data,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action("GetLeaveRequestDetail", "LeaveRequest", new { id }, Request.Scheme)!, "self", "GET"),
                        new LinkDTO(Url.Action("GetLeaveRequests", "LeaveRequest", null, Request.Scheme)!, "leave-request-list", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.LeaveRequestController_Get, ex,
                    "Error occurred while getting detail for LeaveRequestId {LeaveRequestId}", id);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // POST api/leaverequest/create
        [HttpPost("create")]
        [Authorize(Roles = "Admin,Owner,PM,Team_Leader,KCS,Worker")]
        public async Task<ActionResult<RestDTO<LeaveRequestDetailDTO>>> CreateLeaveRequest([FromBody] CreateLeaveRequestDTO? input)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim is null || !int.TryParse(userIdClaim, out var requesterId))
                    return Unauthorized();

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Post,
                    "UserId {UserId} is creating a leave request", requesterId);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Post,
                        "Invalid model state while UserId {UserId} creating a leave request", requesterId);

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

                var created = await _leaveRequestRepo.CreateLeaveRequest(requesterId, input!.Content, input.FromDate, input.ToDate);

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Post,
                    "UserId {UserId} created LeaveRequestId {LeaveRequestId} successfully", requesterId, created.Id);

                var data = new LeaveRequestDetailDTO
                {
                    Id = created.Id,
                    UserId = created.UserId,
                    UserFullName = created.UserFullName,
                    Content = created.Content,
                    DateCreate = created.DateCreate,
                    FromDate = created.FromDate,
                    ToDate = created.ToDate,
                    DateReply = created.DateReply,
                    DenyContent = created.DenyContent,
                    ApprovedByName = created.ApprovedByName,
                    Status = created.StatusName
                };

                return CreatedAtAction(
                    nameof(GetMyLeaveRequestHistoryDetail),
                    new { id = created.Id },
                    new RestDTO<LeaveRequestDetailDTO>
                    {
                        Data = data,
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(Url.Action("GetMyLeaveRequestHistoryDetail", "LeaveRequest", new { id = created.Id }, Request.Scheme)!, "self", "GET"),
                            new LinkDTO(Url.Action("GetMyLeaveRequestHistory", "LeaveRequest", null, Request.Scheme)!, "my-leave-request-history", "GET")
                        }
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(CustomLogEvents.LeaveRequestController_Post, ex,
                    "System configuration error while creating leave request for UserId {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.LeaveRequestController_Post, ex,
                    "Error occurred while creating leave request for UserId {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // PUT api/leaverequest/{id}/deny
        [HttpPut("{id}/deny")]
        [Authorize(Roles = "Owner,PM")]
        public async Task<ActionResult> DenyLeaveRequest(int id, [FromBody] DenyLeaveRequestDTO? input)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim is null || !int.TryParse(userIdClaim, out var approverId))
                    return Unauthorized();

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Put,
                    "Denying LeaveRequestId {LeaveRequestId}", id);

                if (input is null)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Put,
                        "Request body is null while denying LeaveRequestId {LeaveRequestId}", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "body", new[] { "Request body is required." } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (id <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Put,
                        "Invalid LeaveRequestId {LeaveRequestId} - must be greater than 0", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Leave request Id must be greater than 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Put,
                        "Invalid model state while denying LeaveRequestId {LeaveRequestId}", id);

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

                await _leaveRequestRepo.DenyLeaveRequest(id, input!.DenyContent, approverId);

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Put,
                    "LeaveRequestId {LeaveRequestId} denied successfully", id);

                return Ok($"Leave request '{id}' has been denied successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.LeaveRequestController_Put,
                    "LeaveRequestId {LeaveRequestId} not found", id);

                var errorDetails = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                };
                errorDetails.Errors = new Dictionary<string, string[]>
                {
                    { "id", new[] { ex.Message } }
                };
                return StatusCode(StatusCodes.Status404NotFound, errorDetails);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(CustomLogEvents.LeaveRequestController_Put,
                    "LeaveRequestId {LeaveRequestId} cannot be denied - invalid status", id);

                var errorDetails = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                };
                errorDetails.Errors = new Dictionary<string, string[]>
                {
                    { "status", new[] { ex.Message } }
                };
                return StatusCode(StatusCodes.Status403Forbidden, errorDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.LeaveRequestController_Put, ex,
                    "Error occurred while denying LeaveRequestId {LeaveRequestId}", id);

                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        // PUT api/leaverequest/{id}/approve
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Owner,PM")]
        public async Task<ActionResult> ApproveLeaveRequest(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim is null || !int.TryParse(userIdClaim, out var approverId))
                    return Unauthorized();

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Put,
                    "Approving LeaveRequestId {LeaveRequestId}", id);

                if (id <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.LeaveRequestController_Put,
                        "Invalid LeaveRequestId {LeaveRequestId} - must be greater than 0", id);

                    var errorDetails = new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    };
                    errorDetails.Errors = new Dictionary<string, string[]>
                    {
                        { "id", new[] { "Leave request Id must be greater than 0" } }
                    };
                    return StatusCode(StatusCodes.Status400BadRequest, errorDetails);
                }

                await _leaveRequestRepo.ApproveLeaveRequest(id, approverId);

                _logger.LogInformation(CustomLogEvents.LeaveRequestController_Put,
                    "LeaveRequestId {LeaveRequestId} approved successfully", id);

                return Ok($"Leave request '{id}' has been approved successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(CustomLogEvents.LeaveRequestController_Put,
                    "LeaveRequestId {LeaveRequestId} not found", id);

                var errorDetails = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                };
                errorDetails.Errors = new Dictionary<string, string[]>
                {
                    { "id", new[] { ex.Message } }
                };
                return StatusCode(StatusCodes.Status404NotFound, errorDetails);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(CustomLogEvents.LeaveRequestController_Put,
                    "LeaveRequestId {LeaveRequestId} cannot be approved - invalid status", id);

                var errorDetails = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                };
                errorDetails.Errors = new Dictionary<string, string[]>
                {
                    { "status", new[] { ex.Message } }
                };
                return StatusCode(StatusCodes.Status403Forbidden, errorDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.LeaveRequestController_Put, ex,
                    "Error occurred while approving LeaveRequestId {LeaveRequestId}", id);

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