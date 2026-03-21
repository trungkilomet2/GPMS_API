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
using System.Text;
using System.Threading.Tasks;

namespace GPMS.TEST.Api.Controllers
{
    public class WorkerRoleControllerTest
    {
        private readonly Mock<IWorkerRoleRepositories> _mockRepo;
        private readonly Mock<ILogger<AccountController>> _mockLogger;
        private readonly WorkerRoleController _controller;

        public WorkerRoleControllerTest()
        {
            _mockRepo = new Mock<IWorkerRoleRepositories>();
            _mockLogger = new Mock<ILogger<AccountController>>();

            _controller = new WorkerRoleController(
                _mockRepo.Object,
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
                   .Returns("http://localhost/api/workerrole");

            _controller.Url = mockUrl.Object;
        }

        [Fact]
        public async Task GetAllWorkerRoles_ReturnsOk_WhenRolesExist()
        {
            var roles = new List<WorkerSkill>
            {
                new WorkerSkill{ Id = 1, Name = "Tailor"},
                new WorkerSkill{ Id = 2, Name = "Quality Control"}
            };

            _mockRepo.Setup(x => x.GetAllWorkerRoles())
                     .ReturnsAsync(roles);

            var request = new RequestDTO<WorkerSkill>
            {
                PageIndex = 1,
                PageSize = 10
            };

            var result = await _controller.GetAllWorkerRoles(request);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAllWorkerRoles_ReturnsNotFound_WhenNoRoles()
        {
            _mockRepo.Setup(x => x.GetAllWorkerRoles())
                     .ReturnsAsync(new List<WorkerSkill>());

            var request = new RequestDTO<WorkerSkill>();

            var result = await _controller.GetAllWorkerRoles(request);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAllWorkerRoles_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockRepo.Setup(x => x.GetAllWorkerRoles())
                     .ThrowsAsync(new Exception("Database error"));

            var request = new RequestDTO<WorkerSkill>();

            var result = await _controller.GetAllWorkerRoles(request);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateWorkerRole_ReturnsCreated_WhenSuccessful()
        {
            var dto = new CreateWorkerRoleDTO
            {
                Name = "Sewer"
            };

            var createdRole = new WorkerSkill
            {
                Id = 5,
                Name = dto.Name
            };

            _mockRepo.Setup(x => x.CreateWorkerRole(It.IsAny<WorkerSkill>()))
                     .ReturnsAsync(createdRole);

            var result = await _controller.CreateWorkerRole(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateWorkerRole_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("Name", "Required");

            var dto = new CreateWorkerRoleDTO();

            var result = await _controller.CreateWorkerRole(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }
        

        [Fact]
        public async Task CreateWorkerRole_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            var dto = new CreateWorkerRoleDTO
            {
                Name = "Sewer"
            };

            _mockRepo.Setup(x => x.CreateWorkerRole(It.IsAny<WorkerSkill>()))
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.CreateWorkerRole(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
