using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using GPMS.INFRASTRUCTURE.EmailAPI;
using GPMS.TEST.TestCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GPMS.TEST.Api.Controllers;

public class UserControllerTest
{
    private readonly Mock<IUserRepositories> _userRepo = new();
    private readonly Mock<IConfiguration> _config = new();
    private readonly Mock<ILogger<UserController>> _logger = new();
    private readonly Mock<ICloudinaryService> _cloudinary = new();
    private readonly Mock<IMemoryCache> _cache = new();
    private readonly Mock<IEmailRepositories> _email = new();

    private UserController BuildController(int userId = 1)
    {
        var controller = new UserController(_userRepo.Object, _config.Object, _logger.Object, _cloudinary.Object, _cache.Object, _email.Object);
        ControllerTestHelper.AttachHttpContext(controller, ControllerTestHelper.BuildUserWithId(userId));
        return controller;
    }

    private static User BuildFakeUser(int id = 1) => new User
    {
        Id = id,
        UserName = "testuser",
        FullName = "Test User",
        PhoneNumber = "0123456789",
        AvartarUrl = null,
        Location = "HCM",
        Email = "test@mail.com",
        StatusId = 1
    };

    [Fact]
    public async Task GetUser_ReturnsUsers_WhenSuccessful()
    {
        var users = new List<User>
    {
        BuildFakeUser(1),
        BuildFakeUser(2)
    };

        _userRepo.Setup(x => x.GetAllUser()).ReturnsAsync(users);

        var result = await BuildController().GetUser(new RequestDTO<User>());

        var dto = Assert.IsType<RestDTO<IEnumerable<User>>>(result);
        Assert.Equal(2, dto.RecordCount);
    }

    [Fact]
    public async Task GetUser_ReturnsEmpty_WhenNoUsers()
    {
        _userRepo.Setup(x => x.GetAllUser()).ReturnsAsync(new List<User>());

        var result = await BuildController().GetUser(new RequestDTO<User>());

        var dto = Assert.IsType<RestDTO<IEnumerable<User>>>(result);
        Assert.Empty(dto.Data);
    }

    // ─── GetUserListForAdmin ──────────────────────────────────────────────────

    [Fact]
    public async Task GetUserListForAdmin_Returns200_WhenSuccessful()
    {
        _userRepo.Setup(x => x.GetAllUser())
            .ReturnsAsync(new List<User> { BuildFakeUser(1), BuildFakeUser(2) });

        var result = await BuildController().GetUserListForAdmin(new RequestDTO<UserListDTO>());

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<UserListDTO>>>(obj.Value);
        Assert.Equal(2, dto.RecordCount);
    }

    [Fact]
    public async Task GetUserListForAdmin_Returns500_OnException()
    {
        _userRepo.Setup(x => x.GetAllUser()).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetUserListForAdmin(new RequestDTO<UserListDTO>());

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── UpdateUserForAdmin ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateUserForAdmin_Returns200_WhenSuccessful()
    {
        var fakeUser = BuildFakeUser(1);

        _userRepo.Setup(x => x.UpdateUserForAdmin(1, It.IsAny<User>()))
            .ReturnsAsync(fakeUser);

        var input = new UpdatedUserDTO
        {
            FullName = "Updated Name",
            PhoneNumber = "099999999",
            Location = "HN",
            Email = "updated@mail.com"
        };

        var result = await BuildController().UpdateUserForAdmin(1, input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateUserForAdmin_Returns404_WhenUserNotFound()
    {
        _userRepo.Setup(x => x.UpdateUserForAdmin(99, It.IsAny<User>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var input = new UpdatedUserDTO
        {
            FullName = "Updated Name"
        };

        var result = await BuildController().UpdateUserForAdmin(99, input);

        var obj = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateUserForAdmin_Returns500_OnException()
    {
        _userRepo.Setup(x => x.UpdateUserForAdmin(1, It.IsAny<User>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new UpdatedUserDTO
        {
            FullName = "Updated Name"
        };

        var result = await BuildController().UpdateUserForAdmin(1, input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── GetUserDetail ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserDetail_Returns200_WhenFound()
    {
        _userRepo.Setup(x => x.GetUserById(1)).ReturnsAsync(BuildFakeUser());

        var result = await BuildController().GetUserDetail(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(200, obj.StatusCode);
        var dto = Assert.IsType<RestDTO<UserDetailDTO>>(obj.Value);
        Assert.Equal(1, dto.Data.Id);
    }

    [Fact]
    public async Task GetUserDetail_Returns404_WhenNotFound()
    {
        _userRepo.Setup(x => x.GetUserById(99)).ReturnsAsync((User)null);

        var result = await BuildController().GetUserDetail(99);

        var obj = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task GetUserDetail_Returns500_OnException()
    {
        _userRepo.Setup(x => x.GetUserById(1)).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetUserDetail(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── CreateUser ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateUser_Returns201_WhenSuccessful()
    {
        _userRepo.Setup(x => x.CreateNewUser(It.IsAny<User>(), It.IsAny<List<int>>()))
            .ReturnsAsync(BuildFakeUser(id: 5));

        var input = new CreateUserDTO
        {
            UserName = "newuser1",
            Password = "pass123",
            FullName = "New User",
            RoleIds = new List<int> { 1 }
        };

        var result = await BuildController().CreateUser(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, obj.StatusCode);
    }

    [Fact]
    public async Task CreateUser_Returns500_OnException()
    {
        _userRepo.Setup(x => x.CreateNewUser(It.IsAny<User>(), It.IsAny<List<int>>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new CreateUserDTO
        {
            UserName = "newuser1",
            Password = "pass123",
            FullName = "New User",
            RoleIds = new List<int> { 1 }
        };

        var result = await BuildController().CreateUser(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── DisableUser ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DisableUser_Returns200_WhenSuccessful()
    {
        _userRepo.Setup(x => x.DisableAnUser(1)).Returns(Task.CompletedTask);

        var result = await BuildController().DisableUser(1);

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task DisableUser_Returns404_WhenUserNotFound()
    {
        _userRepo.Setup(x => x.DisableAnUser(99))
            .ThrowsAsync(new KeyNotFoundException("User not found."));

        var result = await BuildController().DisableUser(99);

        var obj = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task DisableUser_Returns400_WhenAlreadyInactive()
    {
        _userRepo.Setup(x => x.DisableAnUser(1))
            .ThrowsAsync(new InvalidOperationException("User is already inactive."));

        var result = await BuildController().DisableUser(1);

        var obj = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task DisableUser_Returns500_OnException()
    {
        _userRepo.Setup(x => x.DisableAnUser(1))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().DisableUser(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── AssignRoles ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AssignRoles_Returns200_WhenSuccessful()
    {
        _userRepo.Setup(x => x.AssignRoles(1, It.IsAny<List<int>>())).Returns(Task.CompletedTask);

        var input = new AssignRoleDTO { RoleIds = new List<int> { 1, 2 } };
        var result = await BuildController().AssignRoles(1, input);

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task AssignRoles_Returns404_WhenUserNotFound()
    {
        _userRepo.Setup(x => x.AssignRoles(99, It.IsAny<List<int>>()))
            .ThrowsAsync(new KeyNotFoundException("User not found."));

        var input = new AssignRoleDTO { RoleIds = new List<int> { 1 } };
        var result = await BuildController().AssignRoles(99, input);

        var obj = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task AssignRoles_Returns500_OnException()
    {
        _userRepo.Setup(x => x.AssignRoles(1, It.IsAny<List<int>>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new AssignRoleDTO { RoleIds = new List<int> { 1 } };
        var result = await BuildController().AssignRoles(1, input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── ViewProfile ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ViewProfile_Returns200_WhenFound()
    {
        _userRepo.Setup(x => x.ViewProfile(1)).ReturnsAsync(BuildFakeUser());

        var result = await BuildController(userId: 1).ViewProfile();

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task ViewProfile_Returns404_WhenNotFound()
    {
        _userRepo.Setup(x => x.ViewProfile(1)).ReturnsAsync((User)null);

        var result = await BuildController(userId: 1).ViewProfile();

        var obj = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task ViewProfile_Returns500_OnException()
    {
        _userRepo.Setup(x => x.ViewProfile(1)).ThrowsAsync(new Exception("db error"));

        var result = await BuildController(userId: 1).ViewProfile();

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── UpdateUser ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUser_Returns200_WhenSuccessful()
    {
        var fakeUser = BuildFakeUser(1);

        _userRepo.Setup(x => x.UpdateProfile(1, It.IsAny<User>()))
            .ReturnsAsync(fakeUser);

        var input = new UpdatedUserDTO
        {
            FullName = "Updated User",
            PhoneNumber = "088888888",
            Location = "HN",
            Email = null
        };

        var result = await BuildController(userId: 1).UpdateUser(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_Returns404_WhenUserNotFound()
    {
        _userRepo.Setup(x => x.UpdateProfile(1, It.IsAny<User>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var input = new UpdatedUserDTO
        {
            FullName = "Updated User",
            Email = null
        };

        var result = await BuildController(userId: 1).UpdateUser(input);

        var obj = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_Returns500_OnException()
    {
        _userRepo.Setup(x => x.UpdateProfile(1, It.IsAny<User>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new UpdatedUserDTO
        {
            FullName = "Updated User",
            Email = null
        };

        var result = await BuildController(userId: 1).UpdateUser(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_Returns200_AndSendOtp_WhenEmailProvided()
    {
        var controller = BuildController(userId: 1);

        var input = new UpdatedUserDTO
        {
            Email = "test@gmail.com"
        };

        var result = await controller.UpdateUser(input);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, obj.StatusCode);

        _email.Verify(x => x.SendEmailAsync(
            input.Email,
            It.IsAny<string>(),
            It.IsAny<string>(), EmailType.Verification
        ), Times.Once);

        _userRepo.Verify(x => x.UpdateProfile(It.IsAny<int>(), It.IsAny<User>()), Times.Never);
    }

}
