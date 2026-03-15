using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

public class UserControllerTests
{
    private readonly Mock<IUserRepositories> _repo = new();
    private readonly Mock<IConfiguration> _config = new();
    private readonly Mock<ICloudinaryService> _cloudinary = new();
    private readonly Mock<ILogger<UserController>> _logger = new();

    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController(
            _repo.Object,
            _config.Object,
            _logger.Object,
            _cloudinary.Object
        );

        SetupHttpContext();
        SetupUrlHelper();
    }

    private void SetupHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier,"1")
        });

        httpContext.User = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private void SetupUrlHelper()
    {
        var mockUrl = new Mock<IUrlHelper>();
        mockUrl.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/api/user");

        _controller.Url = mockUrl.Object;
    }

    private static User FakeUser(int id = 1)
    {
        return new User
        {
            Id = id,
            UserName = "testuser",
            FullName = "Test User",
            PhoneNumber = "0123456789",
            Email = "test@mail.com",
            AvartarUrl = "avatar.png",
            Location = "HCM",
            StatusId = 1,
            Roles = new List<Role> { new Role { Name = "Worker" } },
            WorkerRoles = new List<WorkerRole> { new WorkerRole { Name = "Tailor" } }
        };
    }


    [Fact]
    public async Task GetUser_ReturnsUserList()
    {
        var users = new List<User> { FakeUser(), FakeUser(2) };

        _repo.Setup(x => x.GetAllUser())
            .ReturnsAsync(users);

        var result = await _controller.GetUser(new RequestDTO<User>());

        Assert.Equal(2, result.RecordCount);
    }


    [Fact]
    public async Task GetUserDetail_ReturnsOk_WhenUserExists()
    {
        _repo.Setup(x => x.GetUserById(1))
            .ReturnsAsync(FakeUser());

        var result = await _controller.GetUserDetail(1);

        var ok = Assert.IsType<ObjectResult>(result.Result);

        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }

    [Fact]
    public async Task GetUserDetail_ReturnsNotFound_WhenUserNotExist()
    {
        _repo.Setup(x => x.GetUserById(1))
            .ReturnsAsync((User?)null);

        var result = await _controller.GetUserDetail(1);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateUserForAdmin_ReturnsOk()
    {
        _repo.Setup(x =>
            x.UpdateUserForAdmin(1, It.IsAny<User>()))
            .ReturnsAsync(FakeUser());

        var result = await _controller.UpdateUserForAdmin(1, new UpdatedUserDTO());

        var ok = Assert.IsType<ObjectResult>(result.Result);

        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }

    [Fact]
    public async Task UpdateUserForAdmin_ReturnsNotFound()
    {
        _repo.Setup(x =>
            x.UpdateUserForAdmin(1, It.IsAny<User>()))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.UpdateUserForAdmin(1, new UpdatedUserDTO());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }


    [Fact]
    public async Task ViewProfile_ReturnsOk()
    {
        _repo.Setup(x => x.ViewProfile(1))
            .ReturnsAsync(FakeUser());

        var result = await _controller.ViewProfile();

        var ok = Assert.IsType<ObjectResult>(result.Result);

        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }

    [Fact]
    public async Task ViewProfile_ReturnsNotFound()
    {
        _repo.Setup(x => x.ViewProfile(1))
            .ReturnsAsync((User?)null);

        var result = await _controller.ViewProfile();

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }


    [Fact]
    public async Task UpdateUser_Returns200_WhenUpdateSuccessful()
    {
        _repo.Setup(x =>
            x.UpdateProfile(1, It.IsAny<User>()))
            .ReturnsAsync(FakeUser());

        var result = await _controller.UpdateUser(new UpdatedUserDTO());

        var ok = Assert.IsType<ObjectResult>(result.Result);

        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_Returns400_WhenModelInvalid()
    {
        _controller.ModelState.AddModelError("FullName", "Required");

        var result = await _controller.UpdateUser(new UpdatedUserDTO());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateUser_Returns404_WhenUserNotFound()
    {
        _repo.Setup(x =>
            x.UpdateProfile(1, It.IsAny<User>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var result = await _controller.UpdateUser(new UpdatedUserDTO());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateUser_Returns500_WhenExceptionOccurs()
    {
        _repo.Setup(x =>
            x.UpdateProfile(1, It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.UpdateUser(new UpdatedUserDTO());

        var error = Assert.IsType<ObjectResult>(result.Result);

        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
    }
}