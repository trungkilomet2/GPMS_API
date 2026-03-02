using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using GMPS.API.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using GMPS.API.DTOs;

public class UserControllerTests
{
    private readonly Mock<IUserRepositories> _mockRepo;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockRepo = new Mock<IUserRepositories>();
        _controller = new UserController(_mockRepo.Object, null);

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
            Username = "testuser",
            FullName = "Test User",
            PhoneNumber = "0123456789",
            AvartarUrl = "avatar.jpg",
            Email = "test@mail.com"
        };
        _mockRepo.Setup(x => x.ViewProfile(fakeUser.Id))
                 .ReturnsAsync(fakeUser);
        var result = await _controller.ViewProfile(fakeUser.Id);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<RestDTO<User>>(okResult.Value);
        Assert.Equal(fakeUser.Id, returnValue.Data.Id);
        Assert.Single(returnValue.Links);
    }


    [Fact]
    public async Task ViewProfile_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _mockRepo.Setup(x => x.ViewProfile(99))
                 .ReturnsAsync((User)null);
        var result = await _controller.ViewProfile(99);
        Assert.IsType<NotFoundResult>(result.Result);
    }
}