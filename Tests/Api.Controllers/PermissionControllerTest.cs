using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.TEST.TestCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GPMS.TEST.Api.Controllers;

public class PermissionControllerTest
{
    private readonly Mock<IPermissionRepositories> _permissionRepo = new();
    private readonly Mock<ILogEventRepositories> _logEventRepo = new();
    private readonly Mock<ILogger<PermissionController>> _logger = new();

    private PermissionController BuildController()
    {
        var controller = new PermissionController(_permissionRepo.Object, _logEventRepo.Object, _logger.Object);
        ControllerTestHelper.AttachHttpContext(controller);
        return controller;
    }

    private static IEnumerable<PermissionEntry> BuildFakePermissions() =>
    [
        new PermissionEntry { Id = 1, Controller = "User",  Method = "GET", Action = "GetUser",   RoleIds = "1,2" },
        new PermissionEntry { Id = 2, Controller = "Order", Method = "GET", Action = "GetOrders", RoleIds = "2"   }
    ];

    private static Dictionary<string, string> BuildFakeRoleMap() => new()
    {
        { "1", "Admin" },
        { "2", "Owner" }
    };

    // ─── GetPermissions ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetPermissions_Returns200_WhenSuccessful()
    {
        _permissionRepo.Setup(x => x.GetAll()).ReturnsAsync(BuildFakePermissions());
        _permissionRepo.Setup(x => x.GetRoleMap()).ReturnsAsync(BuildFakeRoleMap());

        var result = await BuildController().GetPermissions();

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<PermissionResponseDTO>>>(obj.Value);
        Assert.Equal(2, dto.RecordCount);
    }

    [Fact]
    public async Task GetPermissions_Returns200_WithRolesMapped()
    {
        _permissionRepo.Setup(x => x.GetAll()).ReturnsAsync(BuildFakePermissions());
        _permissionRepo.Setup(x => x.GetRoleMap()).ReturnsAsync(BuildFakeRoleMap());

        var result = await BuildController().GetPermissions();

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<PermissionResponseDTO>>>(obj.Value);
        var first = dto.Data.First();
        Assert.Equal(2, first.Roles.Count);
        Assert.Contains(first.Roles, r => r.Id == 1 && r.Name == "Admin");
        Assert.Contains(first.Roles, r => r.Id == 2 && r.Name == "Owner");
    }

    [Fact]
    public async Task GetPermissions_Returns500_OnException()
    {
        _permissionRepo.Setup(x => x.GetRoleMap()).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetPermissions();

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── UpdatePermission ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePermission_Returns200_WhenSuccessful()
    {
        _permissionRepo.Setup(x => x.GetById(1)).ReturnsAsync(new PermissionEntry { Id = 1, Controller = "User", Method = "GET", Action = "GetUser", RoleIds = "1" });
        _permissionRepo.Setup(x => x.UpdateRoleAuthorize(1, It.IsAny<string?>())).ReturnsAsync(true);

        var result = await BuildController().UpdatePermission(1, new UpdatePermissionDTO { RoleIds = new List<int> { 1, 2 } });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePermission_Returns200_WhenRoleIdsIsEmpty()
    {
        _permissionRepo.Setup(x => x.GetById(1)).ReturnsAsync(new PermissionEntry { Id = 1, Controller = "User", Method = "GET", Action = "GetUser", RoleIds = "1" });
        _permissionRepo.Setup(x => x.UpdateRoleAuthorize(1, null)).ReturnsAsync(true);

        var result = await BuildController().UpdatePermission(1, new UpdatePermissionDTO { RoleIds = new List<int>() });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePermission_Returns400_WhenIdIsZero()
    {
        var result = await BuildController().UpdatePermission(0, new UpdatePermissionDTO());

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task UpdatePermission_Returns400_WhenIdIsNegative()
    {
        var result = await BuildController().UpdatePermission(-1, new UpdatePermissionDTO());

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task UpdatePermission_Returns404_WhenPermissionNotFound()
    {
        _permissionRepo.Setup(x => x.GetById(99)).ReturnsAsync((PermissionEntry?)null);

        var result = await BuildController().UpdatePermission(99, new UpdatePermissionDTO { RoleIds = new List<int> { 1 } });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task UpdatePermission_Returns500_OnException()
    {
        _permissionRepo.Setup(x => x.GetById(1)).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().UpdatePermission(1, new UpdatePermissionDTO { RoleIds = new List<int> { 1, 2 } });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── GetPermissionAuditLogs ──────────────────────────────────────────────

    [Fact]
    public async Task GetPermissionAuditLogs_Returns200_WhenSuccessful()
    {
        var fakeLogs = new List<LogEvent>
        {
            new LogEvent { Id = 1, Message = "PERMISSION_AUDIT PermissionId=1", Level = "Warning", TimeStemp = DateTime.UtcNow }
        };
        _logEventRepo.Setup(x => x.GetPermissionAuditLogs(null, null)).ReturnsAsync(fakeLogs);

        var input = new RequestDTO<LogEventDTO>();
        var result = await BuildController().GetPermissionAuditLogs(input, null, null);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LogEventDTO>>>(obj.Value);
        Assert.Equal(1, dto.RecordCount);
    }

    [Fact]
    public async Task GetPermissionAuditLogs_Returns200_EmptyList_WhenNoLogs()
    {
        _logEventRepo.Setup(x => x.GetPermissionAuditLogs(null, null)).ReturnsAsync(new List<LogEvent>());

        var input = new RequestDTO<LogEventDTO>();
        var result = await BuildController().GetPermissionAuditLogs(input, null, null);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LogEventDTO>>>(obj.Value);
        Assert.Equal(0, dto.RecordCount);
    }

    [Fact]
    public async Task GetPermissionAuditLogs_Returns500_OnException()
    {
        _logEventRepo.Setup(x => x.GetPermissionAuditLogs(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new RequestDTO<LogEventDTO>();
        var result = await BuildController().GetPermissionAuditLogs(input, null, null);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }
}
