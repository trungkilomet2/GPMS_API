using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionRepositories _permissionRepo;
        private readonly ILogEventRepositories _logEventRepo;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(IPermissionRepositories permissionRepo, ILogEventRepositories logEventRepo, ILogger<PermissionController> logger)
        {
            _permissionRepo = permissionRepo ?? throw new ArgumentNullException(nameof(permissionRepo));
            _logEventRepo = logEventRepo ?? throw new ArgumentNullException(nameof(logEventRepo));
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<RestDTO<IEnumerable<PermissionResponseDTO>>>> GetPermissions()
        {
            try
            {
                _logger.LogInformation(CustomLogEvents.PermissionController_Get, "Đang lấy danh sách phân quyền");

                var roleMap = await _permissionRepo.GetRoleMap();
                var permissions = await _permissionRepo.GetAll();

                var data = permissions.Select(p => new PermissionResponseDTO
                {
                    Id = p.Id,
                    Controller = p.Controller,
                    Method = p.Method,
                    Action = p.Action,
                    Roles = string.IsNullOrEmpty(p.RoleIds)
                        ? new List<RoleDTO>()
                        : p.RoleIds.Split(',')
                            .Where(rid => int.TryParse(rid.Trim(), out _) && roleMap.ContainsKey(rid.Trim()))
                            .Select(rid =>
                            {
                                var trimmed = rid.Trim();
                                return new RoleDTO { Id = int.Parse(trimmed), Name = roleMap[trimmed] };
                            })
                            .ToList()
                }).ToList();

                _logger.LogInformation(CustomLogEvents.PermissionController_Get, "Lấy danh sách phân quyền thành công, tổng {Count} mục", data.Count);

                return Ok(new RestDTO<IEnumerable<PermissionResponseDTO>>
                {
                    Data = data,
                    RecordCount = data.Count,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "Permission", null, Request.Scheme)!, "self", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.PermissionController_Get, ex, "Lỗi xảy ra khi lấy danh sách phân quyền");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionDTO input)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning(CustomLogEvents.PermissionController_Put, "ID phân quyền không hợp lệ: {Id}", id);
                    return StatusCode(StatusCodes.Status400BadRequest, new ValidationProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Errors = { { "id", new[] { "ID phải là số nguyên dương." } } }
                    });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.PermissionController_Put, "Lỗi dữ liệu đầu vào khi cập nhật phân quyền với ID {Id}", id);
                    return BadRequest(new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    });
                }

                _logger.LogInformation(CustomLogEvents.PermissionController_Put, "Đang cập nhật phân quyền với ID {Id}", id);

                var existing = await _permissionRepo.GetById(id);
                if (existing == null)
                {
                    _logger.LogWarning(CustomLogEvents.PermissionController_Put, "Không tìm thấy phân quyền với ID {Id}", id);
                    return StatusCode(StatusCodes.Status404NotFound, new ProblemDetails
                    {
                        Detail = $"Không tìm thấy phân quyền với ID '{id}'.",
                        Status = StatusCodes.Status404NotFound,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    });
                }

                var rolesBefore = existing.RoleIds;
                var rolesAfter = input.RoleIds.Count > 0
                    ? string.Join(",", input.RoleIds)
                    : string.Empty;
                var changedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";

                await _permissionRepo.UpdateRoleAuthorize(id, rolesAfter.Length > 0 ? rolesAfter : null);

                _logger.LogInformation(CustomLogEvents.PermissionController_Put, "Cập nhật phân quyền thành công với ID {Id}", id);

                _logger.LogWarning(CustomLogEvents.PermissionController_Audit,
                    "PERMISSION_AUDIT PermissionId={PermissionId} Controller={Controller} Action={Action} ChangedBy={ChangedBy} RolesBefore={RolesBefore} RolesAfter={RolesAfter}",
                    id, existing.Controller, existing.Action, changedByUserId, rolesBefore, rolesAfter);

                return Ok($"Cập nhật phân quyền với ID {id} thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.PermissionController_Put, ex, "Lỗi xảy ra khi cập nhật phân quyền với ID {Id}", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }

        [HttpGet("audit")]
        public async Task<ActionResult<RestDTO<IEnumerable<LogEventDTO>>>> GetPermissionAuditLogs(
            [FromQuery] RequestDTO<LogEventDTO> input,
            [FromQuery] DateTime? fromTimestamp,
            [FromQuery] DateTime? toTimestamp)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(CustomLogEvents.PermissionController_Audit, "Lỗi dữ liệu đầu vào khi lấy lịch sử phân quyền");
                    return BadRequest(new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    });
                }

                _logger.LogInformation(CustomLogEvents.PermissionController_Audit, "Đang lấy lịch sử thay đổi phân quyền");

                var result = await _logEventRepo.GetPermissionAuditLogs(fromTimestamp, toTimestamp);

                if (!string.IsNullOrEmpty(input.FilterQuery?.Trim()))
                {
                    result = result.Where(x =>
                        (x.Message != null && x.Message.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (x.Properties != null && x.Properties.Contains(input.FilterQuery, StringComparison.OrdinalIgnoreCase)));
                }

                var recordCount = result.Count();
                var data = result
                    .OrderByDescending(x => x.TimeStemp)
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize)
                    .Select(x => new LogEventDTO
                    {
                        Id = x.Id,
                        Message = x.Message,
                        MessageTemplate = x.MessageTemplate,
                        Level = x.Level,
                        TimeStemp = x.TimeStemp,
                        Exception = x.Exception,
                        Properties = x.Properties
                    })
                    .ToList();

                _logger.LogInformation(CustomLogEvents.PermissionController_Audit, "Lấy lịch sử phân quyền thành công, tổng {Count} mục", data.Count);

                return Ok(new RestDTO<IEnumerable<LogEventDTO>>
                {
                    Data = data,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(Url.Action(null, "Permission", new { input.PageIndex, input.PageSize }, Request.Scheme)!, "self", "GET")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(CustomLogEvents.PermissionController_Audit, ex, "Lỗi xảy ra khi lấy lịch sử phân quyền");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
            }
        }
    }
}
