using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.TEST.Api.Controllers
{
    public class CommentControllerTest
    {
        private readonly Mock<ICommentRepositories> _mockRepo;
        private readonly Mock<IOrderRepositories> _mockOrder;
        private readonly Mock<ILogger<CommentController>> _mockLogger;
        private readonly Mock<IUserRepositories> _mockUser;
        private readonly CommentController _controller;

        public CommentControllerTest()
        {
            _mockRepo = new Mock<ICommentRepositories>();
            _mockOrder = new Mock<IOrderRepositories>();
            _mockLogger = new Mock<ILogger<CommentController>>();
            _mockUser = new Mock<IUserRepositories>();
            _controller = new CommentController(
                _mockRepo.Object,
                null,
                _mockLogger.Object, _mockUser.Object,_mockOrder.Object
            );
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                      {
                       new Claim(ClaimTypes.NameIdentifier, "1")
                      }, "mock"));
            var httpContext = new DefaultHttpContext
            {
                User = user
            };
            httpContext.Request.Scheme = "http";

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var mockUrl = new Mock<IUrlHelper>();
            mockUrl.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                   .Returns("http://localhost/api/comment");

            _controller.Url = mockUrl.Object;
        }
        [Fact]
        public async Task CreateComment_ReturnsCreated_WhenSuccessful()
        {
            var dto = new CreatedCommentDTO
            {
                FromUserId = 1,
                ToOrderId = 10,
                Content = "Test comment"
            };

            var createdComment = new Comment
            {
                Id = 5,
                fromUserId = dto.FromUserId,
                toOrderId = dto.ToOrderId,
                Content = dto.Content,
                SendDateTime = DateTime.UtcNow
            };

            _mockRepo.Setup(x => x.CreateComment(1,It.IsAny<Comment>()))
                     .ReturnsAsync(createdComment);

            var result = await _controller.CreateComment(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateComment_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("Content", "Required");

            var dto = new CreatedCommentDTO();

            var result = await _controller.CreateComment(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateComment_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            var dto = new CreatedCommentDTO
            {
                FromUserId = 1,
                ToOrderId = 10,
                Content = "Test comment"
            };

            _mockRepo.Setup(x => x.CreateComment(1,It.IsAny<Comment>()))
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.CreateComment(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
