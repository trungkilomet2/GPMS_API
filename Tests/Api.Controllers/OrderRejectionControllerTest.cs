using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.EmailAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace GPMS.TEST.Api.Controllers
{
    public class OrderRejectionControllerTest
    {
        private readonly Mock<IOrderRejectRepositories> _mockRejectRepo;
        private readonly Mock<IEmailRepositories> _mockEmailRepo;
        private readonly Mock<IUserRepositories> _mockUserRepo;
        private readonly Mock<IOrderRepositories> _mockOrderRepo;
        private readonly Mock<ILogger<OrderRejectController>> _mockLogger;

        private readonly OrderRejectController _controller;

        public OrderRejectionControllerTest()
        {
            _mockRejectRepo = new Mock<IOrderRejectRepositories>();
            _mockEmailRepo = new Mock<IEmailRepositories>();
            _mockUserRepo = new Mock<IUserRepositories>();
            _mockOrderRepo = new Mock<IOrderRepositories>();
            _mockLogger = new Mock<ILogger<OrderRejectController>>();

            _controller = new OrderRejectController(
                _mockRejectRepo.Object,
                _mockLogger.Object,
                _mockEmailRepo.Object,
                _mockUserRepo.Object,
                _mockOrderRepo.Object
            );

            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            httpContext.User = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }


        [Fact]
        public async Task CreateOrderReject_Returns201_WhenSuccessful()
        {
            var dto = new CreateOrderRejectDTO
            {
                OrderId = 5,
                Reason = "Invalid design"
            };

            var reject = new OrderRejectReason
            {
                OrderId = 5,
                UserId = 1,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            var order = new Order { Id = 5, UserId = 10 };
            var user = new User { Id = 10, Email = "test@mail.com" };

            _mockRejectRepo.Setup(x => x.CreateReason(It.IsAny<OrderRejectReason>()))
                           .ReturnsAsync(reject);

            _mockOrderRepo.Setup(x => x.GetOrderDetail(dto.OrderId))
                          .ReturnsAsync(order);

            _mockUserRepo.Setup(x => x.GetUserById(order.UserId))
                         .ReturnsAsync(user);

            _mockEmailRepo.Setup(x => x.SendEmailAsync(
                user.Email,
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns(Task.CompletedTask);

            var result = await _controller.CreateOrderReject(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrderReject_Returns400_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("Reason", "Required");

            var result = await _controller.CreateOrderReject(new CreateOrderRejectDTO());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrderReject_Returns500_WhenException()
        {
            var dto = new CreateOrderRejectDTO
            {
                OrderId = 5,
                Reason = "Error case"
            };

            _mockRejectRepo.Setup(x => x.CreateReason(It.IsAny<OrderRejectReason>()))
                           .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.CreateOrderReject(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrderModification_Returns201_WhenSuccessful()
        {
            var dto = new CreateOrderRejectDTO
            {
                OrderId = 7,
                Reason = "Need changes"
            };

            var reject = new OrderRejectReason
            {
                OrderId = 7,
                UserId = 1,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            var order = new Order { Id = 7, UserId = 20 };
            var user = new User { Id = 20, Email = "modify@mail.com" };

            _mockRejectRepo.Setup(x => x.CreateReason(It.IsAny<OrderRejectReason>()))
                           .ReturnsAsync(reject);

            _mockOrderRepo.Setup(x => x.GetOrderDetail(dto.OrderId))
                          .ReturnsAsync(order);

            _mockUserRepo.Setup(x => x.GetUserById(order.UserId))
                         .ReturnsAsync(user);

            _mockEmailRepo.Setup(x => x.SendEmailAsync(
                user.Email,
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns(Task.CompletedTask);

            var result = await _controller.CreateOrderModification(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrderModification_Returns400_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("Reason", "Required");

            var result = await _controller.CreateOrderModification(new CreateOrderRejectDTO());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrderModification_Returns500_WhenException()
        {
            var dto = new CreateOrderRejectDTO
            {
                OrderId = 7,
                Reason = "Error case"
            };

            _mockRejectRepo.Setup(x => x.CreateReason(It.IsAny<OrderRejectReason>()))
                           .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.CreateOrderModification(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}