using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.TEST.TestCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GPMS.TEST.Api.Controllers;

public class RoleControllerTest
{
    private readonly Mock<IPermissionRepositories> _permissionRepo = new();
    private readonly Mock<ILogger<RoleController>> _logger = new();

    private RoleController BuildController()
    {
        var controller = new RoleController(_permissionRepo.Object, _logger.Object);
        ControllerTestHelper.AttachHttpContext(controller);
        return controller;
    }

    private static IEnumerable<Role> BuildFakeRoles() =>
    [
        new Role { Id = 1, Name = "Admin" },
        new Role { Id = 2, Name = "Owner" },
        new Role { Id = 3, Name = "PM" }
    ];

    // ─── GetAllRoles ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllRoles_Returns200_WhenSuccessful()
    {
        _permissionRepo.Setup(x => x.GetAllRoles()).ReturnsAsync(BuildFakeRoles());

        var result = await BuildController().GetAllRoles();

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<RoleDTO>>>(obj.Value);
        Assert.Equal(3, dto.RecordCount);
    }

    [Fact]
    public async Task GetAllRoles_Returns200_WithCorrectMapping()
    {
        _permissionRepo.Setup(x => x.GetAllRoles()).ReturnsAsync(BuildFakeRoles());

        var result = await BuildController().GetAllRoles();

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<RoleDTO>>>(obj.Value);
        Assert.Contains(dto.Data, r => r.Id == 1 && r.Name == "Admin");
        Assert.Contains(dto.Data, r => r.Id == 3 && r.Name == "PM");
    }

    [Fact]
    public async Task GetAllRoles_Returns200_EmptyList_WhenNoRoles()
    {
        _permissionRepo.Setup(x => x.GetAllRoles()).ReturnsAsync(new List<Role>());

        var result = await BuildController().GetAllRoles();

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<RoleDTO>>>(obj.Value);
        Assert.Equal(0, dto.RecordCount);
    }

    [Fact]
    public async Task GetAllRoles_Returns500_OnException()
    {
        _permissionRepo.Setup(x => x.GetAllRoles()).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetAllRoles();

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }
}
