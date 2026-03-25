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
    private readonly Mock<ILogger<PermissionController>> _logger = new();

    private PermissionController BuildController()
    {
        var controller = new PermissionController(_permissionRepo.Object, _logger.Object);
        ControllerTestHelper.AttachHttpContext(controller);
        return controller;
    }

    private static IEnumerable<PermissionEntry> BuildFakePermissions() =>
    [
        new PermissionEntry(1, "User",  "GET", "GetUser",   "1,2"),
        new PermissionEntry(2, "Order", "GET", "GetOrders", "2")
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

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPermissions_Returns500_OnException()
    {
        _permissionRepo.Setup(x => x.GetRoleMap()).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetPermissions();

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── UpdatePermission ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePermission_Returns200_WhenSuccessful()
    {
        _permissionRepo.Setup(x => x.UpdateRoleAuthorize(1, It.IsAny<string?>())).ReturnsAsync(true);

        var result = await BuildController().UpdatePermission(1, new UpdatePermissionDTO { RoleAuthorize = "1,2" });

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdatePermission_Returns200_WhenRoleAuthorizeIsNull()
    {
        _permissionRepo.Setup(x => x.UpdateRoleAuthorize(1, null)).ReturnsAsync(true);

        var result = await BuildController().UpdatePermission(1, new UpdatePermissionDTO { RoleAuthorize = null });

        Assert.IsType<OkResult>(result);
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
        _permissionRepo.Setup(x => x.UpdateRoleAuthorize(99, It.IsAny<string?>())).ReturnsAsync(false);

        var result = await BuildController().UpdatePermission(99, new UpdatePermissionDTO { RoleAuthorize = "1" });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task UpdatePermission_Returns500_OnException()
    {
        _permissionRepo.Setup(x => x.UpdateRoleAuthorize(1, It.IsAny<string?>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().UpdatePermission(1, new UpdatePermissionDTO { RoleAuthorize = "1,2" });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }
}
