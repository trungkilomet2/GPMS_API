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
using System.Text;
using System.Threading.Tasks;

namespace GPMS.TEST
{
    public class OrderControllerTest
    {
        private readonly Mock<IOrderRepositories> _mockRepo;
        private readonly Mock<ILogger<OrderController>> _mockLogger;
        private readonly OrderController _controller;

        public OrderControllerTest()
        {
            _mockRepo = new Mock<IOrderRepositories>();
            _mockLogger = new Mock<ILogger<OrderController>>();

            _controller = new OrderController(
                _mockRepo.Object,
                _mockLogger.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http";

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreated_WhenSuccessful()
        {
            var dto = new CreateOrderDTO
            {
                UserId = 1,
                OrderName = "Test Order",
                Type = "Clothes",
                Size = "L",
                Color = "Red",
                Quantity = 10,
                Cpu = 100,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                Note = "Test note",
                Materials = new List<CreateMaterialDTO>
                {
                    new CreateMaterialDTO
                    {
                        MaterialName = "vải",
                        Uom = "Cái"
                    }
                },
                Templates = new List<CreateTemplateDTO>
                {
                    new CreateTemplateDTO
                    {
                        TemplateName = "Áo thun",
                        File = "Cái"
                    }
                }
            };

            var createdOrder = new Order
            {
                Id = 5,
                UserId = dto.UserId,
                OrderName = dto.OrderName
            };

            _mockRepo.Setup(x => x.CreateOrder(It.IsAny<Order>()))
                     .ReturnsAsync(createdOrder);

            var result = await _controller.CreateOrder(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("OrderName", "Required");

            var dto = new CreateOrderDTO();

            var result = await _controller.CreateOrder(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            var dto = new CreateOrderDTO
            {
                UserId = 1,
                OrderName = "Test Order",
                Type = "Clothes",
                Quantity = 10
            };

            _mockRepo.Setup(x => x.CreateOrder(It.IsAny<Order>()))
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.CreateOrder(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
