using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using GPMS.INFRASTRUCTURE.EmailAPI;
using GPMS.TEST.TestCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace GPMS.TEST.Api.Controllers;

public class OrderControllerTest
{
    private readonly Mock<IOrderRepositories> _orderRepo = new();
    private readonly Mock<ILogger<OrderController>> _logger = new();
    private readonly Mock<ICloudinaryService> _cloudinary = new();
    private readonly Mock<IEmailRepositories> _emailRepo = new();
    private readonly Mock<IUserRepositories> _userRepo = new();

    private OrderController BuildController(int userId = 1)
    {
        var controller = new OrderController(
            _orderRepo.Object,
            _logger.Object,
            _cloudinary.Object,
            _emailRepo.Object,
            _userRepo.Object
        );

        ControllerTestHelper.AttachHttpContext(
            controller,
            ControllerTestHelper.BuildUserWithId(userId)
        );

        return controller;
    }

    private OrderController BuildControllerWithRole(int userId, string role)
    {
        var controller = new OrderController(
            _orderRepo.Object,
            _logger.Object,
            _cloudinary.Object,
            _emailRepo.Object,
            _userRepo.Object
        );

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "tester"),
            new Claim(ClaimTypes.Role, role)
        }, "TestAuth");

        ControllerTestHelper.AttachHttpContext(controller, new ClaimsPrincipal(identity));
        return controller;
    }

    private static Order BuildFakeOrder(
        int id = 1,
        int userId = 1,
        string statusName = OrderStatus_Constants.Pending) => new Order
        {
            Id = id,
            UserId = userId,
            OrderName = "Test Order",
            Quantity = 10,
            Cpu = 100,
            StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
            StatusName = statusName,
            Templates = new List<OTemplate>(),
            Materials = new List<OMaterial>(),
            Histories = new List<OHistoryUpdate>()
        };

    // ─── GetOrders ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrders_Returns200_WhenSuccessful()
    {
        _orderRepo.Setup(x => x.GetAllOrders())
            .ReturnsAsync(new List<Order> { BuildFakeOrder(1), BuildFakeOrder(2) });

        var result = await BuildController().GetOrders(new OrderRequestDTO());

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<OrderListDTO>>>(obj.Value);
        Assert.Equal(2, dto.RecordCount);
    }

    [Fact]
    public async Task GetOrders_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetAllOrders()).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetOrders(new OrderRequestDTO());

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrders_Returns404_WhenPageIndexOutOfRange()
    {
        _orderRepo.Setup(x => x.GetAllOrders())
            .ReturnsAsync(new List<Order> { BuildFakeOrder() });

        var input = new OrderRequestDTO { PageIndex = 99, PageSize = 10 };
        var result = await BuildController().GetOrders(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrders_Returns400_WhenStatusIsInvalid()
    {
        var input = new OrderRequestDTO { Status = "InvalidStatus" };

        var result = await BuildController().GetOrders(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrders_Returns400_WhenDateRangeInvalid()
    {
        var input = new OrderRequestDTO
        {
            StartDateFrom = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            StartDateTo = DateOnly.FromDateTime(DateTime.Today)
        };

        var result = await BuildController().GetOrders(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrders_Returns200_WhenFilterByLowercaseStatus()
    {
        _orderRepo.Setup(x => x.GetAllOrders())
            .ReturnsAsync(new List<Order>
            {
                BuildFakeOrder(1, statusName: OrderStatus_Constants.Pending),
                BuildFakeOrder(2, statusName: OrderStatus_Constants.Approved)
            });

        var input = new OrderRequestDTO { Status = OrderStatus_Constants.Pending.ToLowerInvariant() };

        var result = await BuildController().GetOrders(input);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<OrderListDTO>>>(obj.Value);
        Assert.Equal(1, dto.RecordCount);
    }

    // ─── GetMyOrders ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyOrders_Returns200_WhenSuccessful()
    {
        _orderRepo.Setup(x => x.GetOrdersByUserId(1))
            .ReturnsAsync(new List<Order> { BuildFakeOrder() });

        var result = await BuildController(userId: 1).GetMyOrders(new OrderRequestDTO());

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<OrderListDTO>>>(obj.Value);
        Assert.Equal(1, dto.RecordCount);
    }

    [Fact]
    public async Task GetMyOrders_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetOrdersByUserId(It.IsAny<int>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetMyOrders(new OrderRequestDTO());

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyOrders_Returns400_WhenStatusIsInvalid()
    {
        var input = new OrderRequestDTO { Status = "InvalidStatus" };

        var result = await BuildController(userId: 1).GetMyOrders(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyOrders_Returns400_WhenDateRangeInvalid()
    {
        var input = new OrderRequestDTO
        {
            StartDateFrom = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            StartDateTo = DateOnly.FromDateTime(DateTime.Today)
        };

        var result = await BuildController(userId: 1).GetMyOrders(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyOrders_Returns200_WhenFilterByLowercaseStatus()
    {
        _orderRepo.Setup(x => x.GetOrdersByUserId(1))
            .ReturnsAsync(new List<Order>
            {
                BuildFakeOrder(1, userId: 1, statusName: OrderStatus_Constants.Pending),
                BuildFakeOrder(2, userId: 1, statusName: OrderStatus_Constants.Approved)
            });

        var input = new OrderRequestDTO { Status = OrderStatus_Constants.Pending.ToLowerInvariant() };

        var result = await BuildController(userId: 1).GetMyOrders(input);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<OrderListDTO>>>(obj.Value);
        Assert.Equal(1, dto.RecordCount);
    }

    // ─── GetOrderDetail ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrderDetail_Returns200_WhenFound()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(BuildFakeOrder());

        var result = await BuildController().GetOrderDetail(1);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<OrderDetailDTO>>(obj.Value);
        Assert.Equal(1, dto.Data.Id);
    }

    [Fact]
    public async Task GetOrderDetail_Returns400_WhenIdInvalid()
    {
        var result = await BuildController().GetOrderDetail(0);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrderDetail_Returns404_WhenNotFound()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(99)).ReturnsAsync((Order)null);

        var result = await BuildController().GetOrderDetail(99);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrderDetail_Returns403_WhenCustomerViewsOtherUsersOrder()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(BuildFakeOrder(userId: 99));

        var result = await BuildControllerWithRole(userId: 1, role: "Customer").GetOrderDetail(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrderDetail_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetOrderDetail(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── GetOrderHistory ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrderHistory_Returns200_WhenFound()
    {
        var order = BuildFakeOrder();
        order.Histories = new List<OHistoryUpdate>
        {
            new OHistoryUpdate { FieldName = "OrderName", OldValue = "A", NewValue = "B" }
        };
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(order);

        var result = await BuildController().GetOrderHistory(1);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<OHistoryUpdate>>>(obj.Value);
        Assert.Equal(1, dto.RecordCount);
    }

    [Fact]
    public async Task GetOrderHistory_Returns400_WhenIdInvalid()
    {
        var result = await BuildController().GetOrderHistory(0);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrderHistory_Returns404_WhenNotFound()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(99)).ReturnsAsync((Order)null);

        var result = await BuildController().GetOrderHistory(99);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrderHistory_Returns404_WhenHistoryEmpty()
    {
        var order = BuildFakeOrder();
        order.Histories = new List<OHistoryUpdate>();
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(order);

        var result = await BuildController().GetOrderHistory(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrderHistory_Returns403_WhenCustomerViewsOtherUsersOrderHistory()
    {
        var order = BuildFakeOrder(userId: 99);
        order.Histories = new List<OHistoryUpdate>
        {
            new OHistoryUpdate { FieldName = "OrderName", OldValue = "A", NewValue = "B" }
        };
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(order);

        var result = await BuildControllerWithRole(userId: 1, role: "Customer").GetOrderHistory(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task GetOrderHistory_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetOrderHistory(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }


    [Fact]
    public async Task CreateOrder_Returns201_WhenSuccessful()
    {
        var fakeOrder = BuildFakeOrder(id: 5);

        _orderRepo.Setup(x => x.CreateOrder(It.IsAny<Order>()))
            .ReturnsAsync(fakeOrder);

        _userRepo.Setup(x => x.GetOwner())
            .ReturnsAsync(new User { Id = 2, Email = "owner@mail.com" });

        _emailRepo.Setup(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(), EmailType.OrderNotification
        )).Returns(Task.CompletedTask);

        var input = new CreateOrderDTO
        {
            UserId = 1,
            OrderName = "New Order",
            StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
            Quantity = 10
        };

        var result = await BuildController().CreateOrder(input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, obj.StatusCode);

        _emailRepo.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(), EmailType.OrderNotification
        ), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateOrder_Returns400_WhenModelInvalid()
    {
        var controller = BuildController();
        controller.ModelState.AddModelError("OrderName", "Required");

        var result = await controller.CreateOrder(new CreateOrderDTO());

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.CreateOrder(It.IsAny<Order>()))
            .ThrowsAsync(new Exception("db error"));

        _userRepo.Setup(x => x.GetOwner())
            .ReturnsAsync(new User { Id = 2, Email = "owner@mail.com" });

        var input = new CreateOrderDTO
        {
            UserId = 1,
            OrderName = "New Order",
            StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
            Quantity = 10
        };

        var result = await BuildController().CreateOrder(input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── AddMaterial ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AddMaterial_Returns201_WhenSuccessful()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1))
            .ReturnsAsync(BuildFakeOrder(1, userId: 1));
        _orderRepo.Setup(x => x.AddMaterial(1, It.IsAny<OMaterial>()))
            .ReturnsAsync(new OMaterial { Id = 10, Name = "Cotton", OrderId = 1 });

        var input = new CreateMaterialDTO { MaterialName = "Cotton", Value = 5, Uom = "m" };

        var result = await BuildController().AddMaterial(1, input, null);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, obj.StatusCode);
    }

    [Fact]
    public async Task AddMaterial_Returns400_WhenOrderApproved()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1))
            .ReturnsAsync(BuildFakeOrder(1, userId: 1));
        _orderRepo.Setup(x => x.AddMaterial(1, It.IsAny<OMaterial>()))
            .ThrowsAsync(new InvalidOperationException("Cannot add material to an approved order."));

        var input = new CreateMaterialDTO { MaterialName = "Cotton", Value = 5, Uom = "m" };

        var result = await BuildController().AddMaterial(1, input, null);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task AddMaterial_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1))
            .ReturnsAsync(BuildFakeOrder(1, userId: 1));
        _orderRepo.Setup(x => x.AddMaterial(1, It.IsAny<OMaterial>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new CreateMaterialDTO { MaterialName = "Cotton", Value = 5, Uom = "m" };

        var result = await BuildController().AddMaterial(1, input, null);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── UpdateOrder ──────────────────────────────────────────────────────────

    private static UpdateOrderDTO BuildUpdateOrderDTO(
        DateOnly? startDate = null,
        DateOnly? endDate = null) => new UpdateOrderDTO
        {
            OrderName = "Updated",
            Type = "Shirt",
            Color = "Blue",
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
            EndDate = endDate ?? DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
            Quantity = 5
        };

    [Fact]
    public async Task UpdateOrder_Returns200_WhenSuccessful()
    {
        var existing = BuildFakeOrder(userId: 1, statusName: OrderStatus_Constants.Modification);
        _orderRepo.Setup(x => x.UpdateOrder(1, 1, It.IsAny<UpdateOrderInput>()))
            .ReturnsAsync(existing);

        var result = await BuildController(userId: 1).UpdateOrder(1, BuildUpdateOrderDTO());

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_Returns400_WhenIdInvalid()
    {
        var result = await BuildController().UpdateOrder(0, new UpdateOrderDTO());

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_Returns404_WhenOrderNotFound()
    {
        _orderRepo.Setup(x => x.UpdateOrder(99, It.IsAny<int>(), It.IsAny<UpdateOrderInput>()))
            .ThrowsAsync(new KeyNotFoundException("Order with id '99' not exist in system."));

        var result = await BuildController().UpdateOrder(99, BuildUpdateOrderDTO());

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_Returns403_WhenStatusNotModification()
    {
        _orderRepo.Setup(x => x.UpdateOrder(1, It.IsAny<int>(), It.IsAny<UpdateOrderInput>()))
            .ThrowsAsync(new InvalidOperationException($"Only modify order with status '{OrderStatus_Constants.Modification}'."));

        var result = await BuildController(userId: 1).UpdateOrder(1, BuildUpdateOrderDTO());

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_Returns403_WhenUserIsNotOwner()
    {
        _orderRepo.Setup(x => x.UpdateOrder(1, It.IsAny<int>(), It.IsAny<UpdateOrderInput>()))
            .ThrowsAsync(new UnauthorizedAccessException("You don't have permission to update this order."));

        var result = await BuildController(userId: 1).UpdateOrder(1, BuildUpdateOrderDTO());

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateOrder_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.UpdateOrder(1, It.IsAny<int>(), It.IsAny<UpdateOrderInput>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().UpdateOrder(1, BuildUpdateOrderDTO());

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public async Task DenyOrder_Returns200_WhenSuccessful()
    {
        _orderRepo.Setup(x => x.DenyOrder(1, 1))
            .ReturnsAsync(BuildFakeOrder());

        var result = await BuildController(userId: 1).DenyOrder(1);

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task DenyOrder_Returns400_WhenOrderIdInvalid()
    {
        var result = await BuildController().DenyOrder(0);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task DenyOrder_Returns404_WhenOrderNotFound()
    {
        _orderRepo.Setup(x => x.DenyOrder(It.IsAny<int>(), 99))
            .ThrowsAsync(new KeyNotFoundException("Order with id '99' not exist in system."));

        var result = await BuildController().DenyOrder(99);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task DenyOrder_Returns403_WhenUserIsNotOwner()
    {
        _orderRepo.Setup(x => x.DenyOrder(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("You don't have permission to deny this order."));

        var result = await BuildController(userId: 1).DenyOrder(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DenyOrder_Returns403_WhenStatusNotPending()
    {
        _orderRepo.Setup(x => x.DenyOrder(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Only modify order with status Chờ Xét Duyệt."));

        var result = await BuildController().DenyOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task DenyOrder_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.DenyOrder(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().DenyOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── ApproveOrder ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveOrder_Returns200_WhenSuccessful()
    {
        _orderRepo.Setup(x => x.ApproveOrder(1))
            .ReturnsAsync(BuildFakeOrder(statusName: OrderStatus_Constants.Approved));

        var result = await BuildController().ApproveOrder(1);

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns400_WhenOrderIdInvalid()
    {
        var result = await BuildController().ApproveOrder(0);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns404_WhenOrderNotFound()
    {
        _orderRepo.Setup(x => x.ApproveOrder(99))
            .ThrowsAsync(new KeyNotFoundException("Order with id '99' not exist in system."));

        var result = await BuildController().ApproveOrder(99);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns409_WhenOrderAlreadyApproved()
    {
        _orderRepo.Setup(x => x.ApproveOrder(1))
            .ThrowsAsync(new InvalidOperationException("This order request has already been processed."));

        var result = await BuildController().ApproveOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns409_WhenOrderAlreadyRejected()
    {
        _orderRepo.Setup(x => x.ApproveOrder(1))
            .ThrowsAsync(new InvalidOperationException("This order request has already been processed."));

        var result = await BuildController().ApproveOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns403_WhenStatusNotPending()
    {
        _orderRepo.Setup(x => x.ApproveOrder(1))
            .ThrowsAsync(new InvalidOperationException("Only approve order with status Chờ Xét Duyệt."));

        var result = await BuildController().ApproveOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.ApproveOrder(1))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().ApproveOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }
}
