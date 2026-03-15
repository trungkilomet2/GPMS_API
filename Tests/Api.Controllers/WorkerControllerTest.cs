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
                    WorkerRoles = new List<WorkerRole>{ new WorkerRole{ Name = "Sewer"} },
                    Status = new UserStatus{ Name = "Active"}
                }
            };

                _mockRepo.Setup(x => x.GetAllEmployees())
                         .ReturnsAsync(users);

                var result = await _controller.GetEmployees();

                var okResult = Assert.IsType<OkObjectResult>(result);

                Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            }

            [Fact]
            public async Task GetEmployees_Returns404_WhenNoEmployees()
            {
                _mockRepo.Setup(x => x.GetAllEmployees())
                         .ReturnsAsync(new List<User>());

                var result = await _controller.GetEmployees();

                var objectResult = Assert.IsType<ObjectResult>(result);

                Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
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
                    WorkerRoles = new List<WorkerRole> { new WorkerRole { Name = "Cutting" } }
                };

                _mockRepo.Setup(x => x.GetEmployeeById(1))
                         .ReturnsAsync(user);

                var result = await _controller.GetEmployeeById(1);

                var okResult = Assert.IsType<OkObjectResult>(result);

                Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            }

            [Fact]
            public async Task CreateWorker_ReturnsCreated_WhenSuccessful()
            {
                var dto = new CreateEmployeeDTO
                {
                    UserName = "worker1",
                    Password = "123456",
                    FullName = "Worker One",
                    RoleIds = new List<int> { 2 }
                };

                var createdUser = new User
                {
                    Id = 5,
                    UserName = dto.UserName
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
                    StatusId = 1,
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
