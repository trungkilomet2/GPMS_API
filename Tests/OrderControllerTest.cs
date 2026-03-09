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

            var mockUrl = new Mock<IUrlHelper>();
            mockUrl.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                   .Returns("http://localhost/api/order");
            _controller.Url = mockUrl.Object;
        }

        private static Order BuildFakeOrder(int id = 1, int userId = 1, string status = "Pending") => new Order
        {
            Id = id,
            UserId = userId,
            OrderName = "Test Order",
            Type = "Clothes",
            Size = "L",
            Color = "Red",
            Quantity = 10,
            Cpu = 100,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            Status = status,
            Templates = new List<OTemplate>(),
            Materials = new List<OMaterial>(),
            Histories = new List<OHistoryUpdate>()
        };

        private void SetUserClaims(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
        }

        // CreateOrder
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

        // GetOrders — GET order-list
        [Fact]
        public async Task GetOrders_ReturnsOk_WhenSuccessful()
        {
            var fakeOrders = new List<Order> { BuildFakeOrder(1), BuildFakeOrder(2) };
            _mockRepo.Setup(x => x.GetAllOrders()).ReturnsAsync(fakeOrders);

            var result = await _controller.GetOrders(new RequestDTO<Order>());

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<RestDTO<IEnumerable<OrderListDTO>>>(objectResult.Value);
            Assert.Equal(2, response.RecordCount);
        }

        [Fact]
        public async Task GetOrders_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("PageSize", "Invalid");

            var result = await _controller.GetOrders(new RequestDTO<Order>());

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrders_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockRepo.Setup(x => x.GetAllOrders())
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.GetOrders(new RequestDTO<Order>());

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrders_ReturnsNotFound_WhenPageIndexOutOfRange()
        {
            var fakeOrders = new List<Order> { BuildFakeOrder(1), BuildFakeOrder(2) };
            _mockRepo.Setup(x => x.GetAllOrders()).ReturnsAsync(fakeOrders);

            var input = new RequestDTO<Order> { PageIndex = 99, PageSize = 10 };

            var result = await _controller.GetOrders(input);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrders_ReturnsOk_WhenDataIsEmpty()
        {
            _mockRepo.Setup(x => x.GetAllOrders()).ReturnsAsync(new List<Order>());

            var result = await _controller.GetOrders(new RequestDTO<Order>());

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        // GetMyOrders — GET my-orders/{userId}
        [Fact]
        public async Task GetMyOrders_ReturnsOk_WhenSuccessful()
        {
            var fakeOrders = new List<Order> { BuildFakeOrder(1, userId: 1) };
            _mockRepo.Setup(x => x.GetOrdersByUserId(1)).ReturnsAsync(fakeOrders);

            var result = await _controller.GetMyOrders(1, new RequestDTO<Order>());

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<RestDTO<IEnumerable<OrderListDTO>>>(objectResult.Value);
            Assert.Equal(1, response.RecordCount);
        }

        [Fact]
        public async Task GetMyOrders_ReturnsBadRequest_WhenUserIdIsZero()
        {
            var result = await _controller.GetMyOrders(0, new RequestDTO<Order>());

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMyOrders_ReturnsBadRequest_WhenUserIdIsNegative()
        {
            var result = await _controller.GetMyOrders(-1, new RequestDTO<Order>());

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMyOrders_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("PageSize", "Invalid");

            var result = await _controller.GetMyOrders(1, new RequestDTO<Order>());

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMyOrders_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockRepo.Setup(x => x.GetOrdersByUserId(1)).ThrowsAsync(new Exception("Database error"));

            var result = await _controller.GetMyOrders(1, new RequestDTO<Order>());

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMyOrders_ReturnsNotFound_WhenPageIndexOutOfRange()
        {
            var fakeOrders = new List<Order> { BuildFakeOrder(1, userId: 1) };
            _mockRepo.Setup(x => x.GetOrdersByUserId(1)).ReturnsAsync(fakeOrders);

            var result = await _controller.GetMyOrders(1, new RequestDTO<Order> { PageIndex = 99, PageSize = 10 });

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetMyOrders_ReturnsOk_WhenDataIsEmpty()
        {
            _mockRepo.Setup(x => x.GetOrdersByUserId(1)).ReturnsAsync(new List<Order>());

            var result = await _controller.GetMyOrders(1, new RequestDTO<Order>());

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        // GetOrderDetail — GET order-detail/{id}
        [Fact]
        public async Task GetOrderDetail_ReturnsOk_WhenOrderExists()
        {
            var fakeOrder = BuildFakeOrder(id: 1);
            _mockRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(fakeOrder);

            var result = await _controller.GetOrderDetail(1);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<RestDTO<OrderDetailDTO>>(objectResult.Value);
            Assert.Equal(1, response.Data.Id);
            Assert.Equal("Test Order", response.Data.OrderName);
        }

        [Fact]
        public async Task GetOrderDetail_ReturnsBadRequest_WhenIdIsZero()
        {
            var result = await _controller.GetOrderDetail(0);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrderDetail_ReturnsBadRequest_WhenIdIsNegative()
        {
            var result = await _controller.GetOrderDetail(-5);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrderDetail_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            _mockRepo.Setup(x => x.GetOrderDetail(99)).ReturnsAsync((Order)null);

            var result = await _controller.GetOrderDetail(99);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrderDetail_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockRepo.Setup(x => x.GetOrderDetail(1))
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.GetOrderDetail(1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        // GetOrderHistory — GET {id}/history
        [Fact]
        public async Task GetOrderHistory_ReturnsOk_WithCorrectHistoryCount()
        {
            var fakeOrder = BuildFakeOrder(id: 1);
            fakeOrder.Histories = new List<OHistoryUpdate>
            {
                new OHistoryUpdate { Id = 1, OrderId = 1, FieldName = "Color", OldValue = "Blue", NewValue = "Red" },
                new OHistoryUpdate { Id = 2, OrderId = 1, FieldName = "Quantity", OldValue = "5", NewValue = "10" }
            };
            _mockRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(fakeOrder);

            var result = await _controller.GetOrderHistory(1);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<RestDTO<IEnumerable<OHistoryUpdate>>>(objectResult.Value);
            Assert.Equal(2, response.RecordCount);
        }

        [Fact]
        public async Task GetOrderHistory_ReturnsBadRequest_WhenIdIsZero()
        {
            var result = await _controller.GetOrderHistory(0);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrderHistory_ReturnsBadRequest_WhenIdIsNegative()
        {
            var result = await _controller.GetOrderHistory(-1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrderHistory_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            _mockRepo.Setup(x => x.GetOrderDetail(99)).ReturnsAsync((Order)null);

            var result = await _controller.GetOrderHistory(99);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrderHistory_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockRepo.Setup(x => x.GetOrderDetail(1))
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.GetOrderHistory(1);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        // AddMaterial — POST {orderId}/materials
        [Fact]
        public async Task AddMaterial_ReturnsCreated_WhenSuccessful()
        {
            var dto = new CreateMaterialDTO { MaterialName = "Vải cotton", Value = 10, Uom = "Mét", Note = "Loại tốt" };
            var fakeMaterial = new OMaterial { Id = 1, Name = dto.MaterialName, Value = dto.Value, Uom = dto.Uom };
            _mockRepo.Setup(x => x.AddMaterial(1, It.IsAny<OMaterial>())).ReturnsAsync(fakeMaterial);

            var result = await _controller.AddMaterial(1, dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task AddMaterial_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("MaterialName", "Required");

            var result = await _controller.AddMaterial(1, new CreateMaterialDTO());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task AddMaterial_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            var dto = new CreateMaterialDTO { MaterialName = "Vải", Value = 5, Uom = "Mét" };
            _mockRepo.Setup(x => x.AddMaterial(1, It.IsAny<OMaterial>()))
                     .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.AddMaterial(1, dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        // UpdateOrder — PUT {id}/update
        [Fact]
        public async Task UpdateOrder_ReturnsOk_WhenSuccessful()
        {
            SetUserClaims(userId: 1);
            var existingOrder = BuildFakeOrder(id: 1, userId: 1, status: "Modification");
            _mockRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existingOrder);
            _mockRepo.Setup(x => x.UpdateOrder(1, It.IsAny<Order>(), It.IsAny<List<OHistoryUpdate>>()))
                     .ReturnsAsync(existingOrder);

            var dto = new UpdateOrderDTO
            {
                OrderName = "Updated Order",
                Type = "Clothes",
                Color = "Blue",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Quantity = 20
            };

            var result = await _controller.UpdateOrder(1, dto);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsBadRequest_WhenIdIsZero()
        {
            var result = await _controller.UpdateOrder(0, new UpdateOrderDTO());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsBadRequest_WhenIdIsNegative()
        {
            var result = await _controller.UpdateOrder(-1, new UpdateOrderDTO());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("OrderName", "Required");
            SetUserClaims(userId: 1);

            var result = await _controller.UpdateOrder(1, new UpdateOrderDTO());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsUnauthorized_WhenNoUserClaim()
        {
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            var dto = new UpdateOrderDTO
            {
                OrderName = "Updated Order",
                Type = "Clothes",
                Color = "Blue",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Quantity = 20
            };

            var result = await _controller.UpdateOrder(1, dto);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            SetUserClaims(userId: 1);
            _mockRepo.Setup(x => x.GetOrderDetail(99)).ReturnsAsync((Order)null);

            var dto = new UpdateOrderDTO
            {
                OrderName = "Updated Order",
                Type = "Clothes",
                Color = "Blue",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Quantity = 20
            };

            var result = await _controller.UpdateOrder(99, dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsForbidden_WhenStatusIsNotModification()
        {
            SetUserClaims(userId: 1);
            var existingOrder = BuildFakeOrder(id: 1, userId: 1, status: "Pending");
            _mockRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existingOrder);

            var dto = new UpdateOrderDTO
            {
                OrderName = "Updated Order",
                Type = "Clothes",
                Color = "Blue",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Quantity = 20
            };

            var result = await _controller.UpdateOrder(1, dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsForbidden_WhenUserIsNotOwner()
        {
            SetUserClaims(userId: 2);
            var existingOrder = BuildFakeOrder(id: 1, userId: 1, status: "Modification");
            _mockRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existingOrder);

            var dto = new UpdateOrderDTO
            {
                OrderName = "Updated Order",
                Type = "Clothes",
                Color = "Blue",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Quantity = 20
            };

            var result = await _controller.UpdateOrder(1, dto);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            SetUserClaims(userId: 1);
            var existingOrder = BuildFakeOrder(id: 1, userId: 1, status: "Modification");
            _mockRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existingOrder);
            _mockRepo.Setup(x => x.UpdateOrder(1, It.IsAny<Order>(), It.IsAny<List<OHistoryUpdate>>()))
                     .ThrowsAsync(new Exception("Database error"));

            var dto = new UpdateOrderDTO
            {
                OrderName = "Updated Order",
                Type = "Clothes",
                Color = "Blue",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Quantity = 20
            };

            var result = await _controller.UpdateOrder(1, dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
