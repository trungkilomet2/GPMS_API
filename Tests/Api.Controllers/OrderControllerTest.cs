using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
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
    public class OrderControllerTest
    {
        private readonly Mock<IOrderRepositories> _mockRepo;
        private readonly Mock<ILogger<OrderController>> _mockLogger;
        private readonly Mock<ICloudinaryService> _mockCloudinary;
        private readonly OrderController _controller;

        public OrderControllerTest()
        {
            _mockRepo = new Mock<IOrderRepositories>();
            _mockLogger = new Mock<ILogger<OrderController>>();
            _mockCloudinary = new Mock<ICloudinaryService>();

            _controller = new OrderController(
                _mockRepo.Object,
                _mockLogger.Object,
                _mockCloudinary.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http";

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var mockUrl = new Mock<IUrlHelper>();
            mockUrl.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                   .Returns("http://localhost/api/order");

            _controller.Url = mockUrl.Object;
        }


        [Fact]
        public async Task CreateOrder_ShouldMapMaterialsAndTemplatesCorrectly()
        {
            var dto = new CreateOrderDTO
            {
                UserId = 1,
                OrderName = "Test Order",
                Type = "Shirt",
                Size = "L",
                Color = "Red",
                Quantity = 50,
                Cpu = 22000,
                Note = "Test note",
                StartDate = new DateOnly(2026, 3, 15),
                EndDate = new DateOnly(2026, 3, 20),

                Materials = new List<CreateMaterialDTO>
        {
            new CreateMaterialDTO
            {
                MaterialName = "Fabric",
                Image = "https://tse2.mm.bing.net/th/id/OIP.5zy9t73ebMLan12S5kGAEAHaEK?rs=1&pid=ImgDetMain&o=7&rm=3",
                Value = 100,
                Uom = "m",
                Note = "material note"
            }
        },

                Templates = new List<CreateTemplateDTO>
        {
            new CreateTemplateDTO
            {
                TemplateName = "Template1",
                Type = "PDF",
                File = "https://tse2.mm.bing.net/th/id/OIP.5zy9t73ebMLan12S5kGAEAHaEK?rs=1&pid=ImgDetMain&o=7&rm=3",
                Quantity = 2,
                Note = "template note"
            }
        }
            };

            var createdOrder = new Order
            {
                Id = 99,
                OrderName = dto.OrderName
            };

            _mockRepo.Setup(x => x.CreateOrder(It.IsAny<Order>()))
                     .ReturnsAsync(createdOrder);

            var result = await _controller.CreateOrder(dto);

            _mockRepo.Verify(x => x.CreateOrder(It.Is<Order>(o =>
                o.UserId == dto.UserId &&
                o.OrderName == dto.OrderName &&
                o.Material.Count == 1 &&
                o.Template.Count == 1 &&
                o.Material.First().MaterialName == "Fabric" &&
                o.Template.First().TemplateName == "Template1"
            )), Times.Once);

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
                OrderName = "Test Order"
            };

            _mockRepo.Setup(x => x.CreateOrder(It.IsAny<Order>()))
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.CreateOrder(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task RequestOrderModification_ReturnsOk_WhenSuccessful()
        {
            int orderId = 5;

            var order = new Order
            {
                Id = orderId,
                StatusName = OrderStatus_Constants.Pending
            };

            _mockRepo.Setup(x => x.GetOrderDetail(orderId))
                     .ReturnsAsync(order);

            _mockRepo.Setup(x => x.RequestOrderModification(
                It.IsAny<int>(),
                It.IsAny<Order>(),
                It.IsAny<List<OHistoryUpdate>>()))
                .ReturnsAsync(new Order());

            var result = await _controller.RequestOrderModification(orderId);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task RequestOrderModification_ReturnsBadRequest_WhenOrderIdInvalid()
        {
            var result = await _controller.RequestOrderModification(0);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task RequestOrderModification_ReturnsNotFound_WhenOrderNotExist()
        {
            int orderId = 10;

            _mockRepo.Setup(x => x.GetOrderDetail(orderId))
                     .ReturnsAsync((Order)null);

            var result = await _controller.RequestOrderModification(orderId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task RequestOrderModification_ReturnsForbidden_WhenStatusInvalid()
        {
            int orderId = 10;

            var order = new Order
            {
                Id = orderId,
                StatusName = "Approved"
            };

            _mockRepo.Setup(x => x.GetOrderDetail(orderId))
                     .ReturnsAsync(order);

            var result = await _controller.RequestOrderModification(orderId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        }

        [Fact]
        public async Task RequestOrderModification_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            int orderId = 10;

            _mockRepo.Setup(x => x.GetOrderDetail(orderId))
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.RequestOrderModification(orderId);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
