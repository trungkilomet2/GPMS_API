using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class OrderRejectionControllerTest
    {
        private readonly Mock<IOrderRejectRepositories> _mockRepo;
        private readonly Mock<ILogger<OrderRejectController>> _mockLogger;
        private readonly OrderRejectController _controller;

        public OrderRejectionControllerTest()
        {
            _mockRepo = new Mock<IOrderRejectRepositories>();
            _mockLogger = new Mock<ILogger<OrderRejectController>>();

            _controller = new OrderRejectController(
                _mockRepo.Object,
                _mockLogger.Object
            );

            var httpContext = new DefaultHttpContext();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            httpContext.User = user;

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }


        [Fact]
        public async Task CreateOrderReject_ReturnsCreated_WhenSuccessful()
        {
            var dto = new CreateOrderRejectDTO
            {
                OrderId = 5,
                Reason = "Invalid design",
                CreatedAt = DateTime.UtcNow
            };

            var createdReject = new OrderRejectReason
            {
                OrderId = dto.OrderId,
                UserId = 1,
                Reason = dto.Reason,
                CreatedAt = dto.CreatedAt.Value
            };

            _mockRepo.Setup(x => x.CreateReason(It.IsAny<OrderRejectReason>()))
                     .ReturnsAsync(createdReject);

            var result = await _controller.CreateOrderReject(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrderReject_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("Reason", "Required");

            var dto = new CreateOrderRejectDTO();

            var result = await _controller.CreateOrderReject(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrderReject_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            var dto = new CreateOrderRejectDTO
            {
                OrderId = 5,
                Reason = "Invalid design"
            };

            _mockRepo.Setup(x => x.CreateReason(It.IsAny<OrderRejectReason>()))
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.CreateOrderReject(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
