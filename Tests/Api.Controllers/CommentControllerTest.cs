using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.TEST.TestCommon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GPMS.TEST.Api.Controllers;

public class CommentControllerTest
{
    private readonly Mock<ICommentRepositories> _repo = new();
    private readonly CommentController _controller;

    public CommentControllerTest()
    {
        _controller = new CommentController(_repo.Object,null,null!);
        ControllerTestHelper.AttachHttpContext(_controller);
    }

    [Fact]
    public async Task GetCommentByOrderId_ReturnsList()
    {
        _repo.Setup(x => x.GetCommentById(1)).ReturnsAsync([new Comment { Id = 1, Content = "ok" }]);

        var result = await _controller.GetCommentByOrderId(1);

      //  Assert.Equal(1, result.Coun);
    }

    [Fact]
    public async Task CreateComment_ReturnsCreated_WhenValid()
    {
        _repo.Setup(x => x.CreateComment(It.IsAny<Comment>())).ReturnsAsync(new Comment { Id = 5 });

        var result = await _controller.CreateComment(new CreatedCommentDTO { FromUserId = 1, ToOrderId = 1, Content = "new" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_ReturnsBadRequest_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("id", "required");

        var result = await _controller.DeleteComment(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }
}