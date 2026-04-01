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
    public class WorkerControllerTest
    {
        private readonly Mock<IWorkerRepositories> _mockRepo;
        private readonly Mock<ILogger<AccountController>> _mockLogger;
        private readonly WorkerController _controller;

        public WorkerControllerTest()
        {
            _mockRepo = new Mock<IWorkerRepositories>();
            _mockLogger = new Mock<ILogger<AccountController>>();

            _controller = new WorkerController(
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
                   .Returns("http://localhost/api/worker");

            _controller.Url = mockUrl.Object;
        }

        private void SetUserClaims(int userId, string role = "PM")
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Role, role)
    };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        [Fact]
        public async Task GetEmployees_ReturnsOk_WhenEmployeesExist()
        {
            var users = new List<User>
                 {
        new User
        {
            Id = 1,
            UserName = "worker1",
            FullName = "Worker One",
            Email = "worker@test.com",
            PhoneNumber = "123",
            Roles = new List<Role>{ new Role{ Name = "Worker"} },
            WorkerSkills = new List<WorkerSkill>{ new WorkerSkill{ Name = "Sewer"} },
            Status = new UserStatus{ Name = "Active"}
        }
                 };

            _mockRepo.Setup(x => x.GetAllEmployees())
                     .ReturnsAsync(users);

            var input = new RequestDTO<EmployeeDTO>
            {
                PageIndex = 0,
                PageSize = 10
            };

            var result = await _controller.GetEmployees(input);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var data = Assert.IsType<RestDTO<IEnumerable<EmployeeDTO>>>(okResult.Value);
            Assert.Single(data.Data);
        }

        [Fact]
        public async Task GetEmployeeById_ReturnsBadRequest_WhenIdInvalid()
        {
            var result = await _controller.GetEmployeeById(0);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeById_ReturnsNotFound_WhenEmployeeNotFound()
        {
            _mockRepo.Setup(x => x.GetEmployeeById(1))
                     .ReturnsAsync((User)null);

            var result = await _controller.GetEmployeeById(1);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeById_ReturnsOk_WhenEmployeeExists()
        {
            var user = new User
            {
                Id = 1,
                UserName = "worker1",
                FullName = "Worker One",
                Email = "worker@test.com",
                PhoneNumber = "123",
                Roles = new List<Role> { new Role { Name = "Worker" } },
                WorkerSkills = new List<WorkerSkill> { new WorkerSkill { Name = "Cutting" } }
            };

            _mockRepo.Setup(x => x.GetEmployeeById(1))
                     .ReturnsAsync(user);

            var result = await _controller.GetEmployeeById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetEmployeesbyPMId_ReturnsOk_WhenEmployeesExist()
        {
            var pmId = 99;
            SetUserClaims(pmId);
            var users = new List<User>
        {
            new User
            {
                Id = 1,
                UserName = "worker1",
                FullName = "Worker One",
                Email = "worker1@test.com",
                PhoneNumber = "123",
                ManagerId = pmId,
                Roles = new List<Role> { new Role { Name = "Worker" } },
                WorkerSkills = new List<WorkerSkill> { new WorkerSkill { Name = "Sewer" } },
                Status = new UserStatus { Name = "Active" }
            },
            new User
            {
                Id = 2,
                UserName = "worker2",
                FullName = "Worker Two",
                Email = "worker2@test.com",
                PhoneNumber = "456",
                ManagerId = pmId,
                Roles = new List<Role> { new Role { Name = "Worker" } },
                WorkerSkills = new List<WorkerSkill> { new WorkerSkill { Name = "Cutting" } },
                Status = new UserStatus { Name = "Inactive" }
            }
        };
            _mockRepo.Setup(x => x.GetAllEmployeesByPMId(pmId))
                     .ReturnsAsync(users);
            var input = new RequestDTO<EmployeeDTO>
            {
                PageIndex = 0,
                PageSize = 10
            };
            var result = await _controller.GetEmployeesbyPMId(input);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var data = Assert.IsType<RestDTO<IEnumerable<EmployeeDTO>>>(okResult.Value);
            Assert.Equal(2, data.RecordCount);
            Assert.Equal(2, data.Data.Count());
        }

        [Fact]
        public async Task GetEmployeesbyPMId_ReturnsNotFound_WhenNoEmployeesFound()
        {
            var pmId = 99;
            SetUserClaims(pmId);
            _mockRepo.Setup(x => x.GetAllEmployeesByPMId(pmId))
                     .ReturnsAsync((IEnumerable<User>)null);
            var input = new RequestDTO<EmployeeDTO>
            {
                PageIndex = 0,
                PageSize = 10
            };
            var result = await _controller.GetEmployeesbyPMId(input);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            Assert.Equal("Not found any emloyee for PM", notFoundResult.Value);
        }

        [Fact]
        public async Task GetEmployeesbyPMId_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var pmId = 99;
            SetUserClaims(pmId);
            _controller.ModelState.AddModelError("PageSize", "Invalid PageSize");
            var input = new RequestDTO<EmployeeDTO>
            {
                PageIndex = 0,
                PageSize = 10
            };
            var result = await _controller.GetEmployeesbyPMId(input);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetEmployeesbyPMId_ReturnsInternalServerError_WhenExceptionThrown()
        {
            var pmId = 99;
            SetUserClaims(pmId);

            _mockRepo.Setup(x => x.GetAllEmployeesByPMId(pmId))
                     .ThrowsAsync(new Exception("Database error"));

            var input = new RequestDTO<EmployeeDTO>
            {
                PageIndex = 0,
                PageSize = 10
            };
            var result = await _controller.GetEmployeesbyPMId(input);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal(500, problemDetails.Status);
            Assert.Equal("Database error", problemDetails.Detail);
        }

        [Fact]
        public async Task CreateWorker_ReturnsCreated_WhenSuccessful()
        {
            var dto = new CreateEmployeeDTO
            {
                UserName = "worker1",
                Password = "123456",
                FullName = "Worker One",
                RoleIds = new List<int> { 2 },
                ManagerId = 1,
            };

            var createdUser = new User
            {
                Id = 5,
                UserName = dto.UserName,
                StatusId = 1
            };

            _mockRepo.Setup(x => x.CreateEmployee(It.IsAny<User>()))
                     .ReturnsAsync(createdUser);

            var result = await _controller.CreateWorker(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateWorker_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("UserName", "Required");

            var dto = new CreateEmployeeDTO();

            var result = await _controller.CreateWorker(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateWorker_ReturnsBadRequest_WhenInvalidId()
        {
            var dto = new UpdateEmployeeDTO();

            var result = await _controller.UpdateWorker(0, dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateWorker_ReturnsOk_WhenSuccessful()
        {
            var dto = new UpdateEmployeeDTO
            {
                FullName = "Updated Worker",
                RoleIds = new List<int> { 2 }
            };

            var updatedUser = new User { Id = 1 };

            _mockRepo.Setup(x => x.UpdateEmployee(1, It.IsAny<User>()))
                     .ReturnsAsync(updatedUser);

            var result = await _controller.UpdateWorker(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
    }
}
