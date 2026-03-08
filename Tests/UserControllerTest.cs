using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class UserControllerTests
{
    private readonly Mock<IUserRepositories> _mockRepo;
    private readonly UserController _controller;
    private readonly Mock<ILogger<UserController>> _mockLogger;

    public UserControllerTests()
    {
        _mockRepo = new Mock<IUserRepositories>();
        _mockLogger = new Mock<ILogger<UserController>>();

        _controller = new UserController(
            _mockRepo.Object,
            null,
            _mockLogger.Object
        );

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        var mockUrl = new Mock<IUrlHelper>();
        mockUrl.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
               .Returns("http://localhost/api/user/1");

        _controller.Url = mockUrl.Object;
    }

    [Fact]
    public async Task ViewProfile_ReturnsOk_WhenUserExists()
    {
        var fakeUser = new User
        {
            Id = 1, 
            FullName = "Test User",
            PhoneNumber = "0123456789",
            AvartarUrl = "https://tse2.mm.bing.net/th/id/OIP.5zy9t73ebMLan12S5kGAEAHaEK?rs=1&pid=ImgDetMain&o=7&rm=3",
            Location = "HCM",
            Email = "test@mail.com"
        };
        _mockRepo.Setup(x => x.ViewProfile(fakeUser.Id)).ReturnsAsync(fakeUser);
        var result = await _controller.ViewProfile(fakeUser.Id);
        var okResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var returnValue = Assert.IsType<RestDTO<ViewProfileDTO>>(okResult.Value);
        Assert.Equal(fakeUser.FullName, returnValue.Data.FullName);
        Assert.Single(returnValue.Links);
    }


    [Fact]
    public async Task ViewProfile_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _mockRepo.Setup(x => x.ViewProfile(99))
                 .ReturnsAsync((User)null);

        var result = await _controller.ViewProfile(99);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateUser_ReturnsOk_WhenUpdateSuccessful()
    {
        var updateDto = new UpdatedUserDTO
        {
            FullName = "Updated User",
            PhoneNumber = "0999999999",
            AvartarUrl = "avatar.png",
            Location = "Hanoi",
            Email = "updated@mail.com"
        };

        var updatedUser = new User
        {
            Id = 1,
            FullName = updateDto.FullName,
            PhoneNumber = updateDto.PhoneNumber,
            AvartarUrl = updateDto.AvartarUrl,
            Location = updateDto.Location,
            Email = updateDto.Email
        };

        _mockRepo.Setup(x => x.UpdateProfile(1, It.IsAny<User>()))
                 .ReturnsAsync(updatedUser);

        var result = await _controller.UpdateUser(1, updateDto);

        var okResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var returnValue = Assert.IsType<RestDTO<User>>(okResult.Value);

        Assert.Equal(updatedUser.Id, returnValue.Data.Id);
        Assert.Equal(updatedUser.FullName, returnValue.Data.FullName);
        Assert.Single(returnValue.Links);
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequest_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("FullName", "Required");

        var updateDto = new UpdatedUserDTO();

        var result = await _controller.UpdateUser(1, updateDto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);

        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        var updateDto = new UpdatedUserDTO
        {
            FullName = "Test",
            PhoneNumber = "0123456789",
            AvartarUrl = "avatar.png",
            Location = "HCM",
            Email = "test@mail.com"
        };

        _mockRepo.Setup(x => x.UpdateProfile(It.IsAny<int>(), It.IsAny<User>()))
                 .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.UpdateUser(1, updateDto);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);

        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }
}