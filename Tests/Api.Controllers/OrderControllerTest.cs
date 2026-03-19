using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using GPMS.TEST.TestCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GPMS.TEST.Api.Controllers;

public class OrderControllerTest
{
    private readonly Mock<IOrderRepositories> _orderRepo = new();
    private readonly Mock<ILogger<OrderController>> _logger = new();
    private readonly Mock<ICloudinaryService> _cloudinary = new();

    private OrderController BuildController(int userId = 1)
    {
        var controller = new OrderController(_orderRepo.Object, _logger.Object, _cloudinary.Object);
        ControllerTestHelper.AttachHttpContext(controller, ControllerTestHelper.BuildUserWithId(userId));
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
            Type = "Clothes",
            Size = "L",
            Color = "Red",
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
    public async Task GetOrderHistory_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetOrderHistory(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── CreateOrder ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrder_Returns201_WhenSuccessful()
    {
        _orderRepo.Setup(x => x.CreateOrder(It.IsAny<Order>()))
            .ReturnsAsync(BuildFakeOrder(id: 5));

        var input = new CreateOrderDTO
        {
            UserId = 1,
            OrderName = "New Order",
            Type = "Shirt",
            Color = "Blue",
            StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
            Quantity = 10
        };

        var result = await BuildController().CreateOrder(input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, obj.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.CreateOrder(It.IsAny<Order>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new CreateOrderDTO
        {
            UserId = 1,
            OrderName = "New Order",
            Type = "Shirt",
            Color = "Blue",
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
        _orderRepo.Setup(x => x.AddMaterial(1, It.IsAny<OMaterial>()))
            .ReturnsAsync(new OMaterial { Id = 10, Name = "Cotton", OrderId = 1 });

        var input = new CreateMaterialDTO { MaterialName = "Cotton", Value = 5, Uom = "m" };

        var result = await BuildController().AddMaterial(1, input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, obj.StatusCode);
    }

    [Fact]
    public async Task AddMaterial_Returns400_WhenOrderApproved()
    {
        _orderRepo.Setup(x => x.AddMaterial(1, It.IsAny<OMaterial>()))
            .ThrowsAsync(new InvalidOperationException("Cannot add material to an approved order."));

        var input = new CreateMaterialDTO { MaterialName = "Cotton", Value = 5, Uom = "m" };

        var result = await BuildController().AddMaterial(1, input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task AddMaterial_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.AddMaterial(1, It.IsAny<OMaterial>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new CreateMaterialDTO { MaterialName = "Cotton", Value = 5, Uom = "m" };

        var result = await BuildController().AddMaterial(1, input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── UpdateOrder ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateOrder_Returns200_WhenSuccessful()
    {
        var existing = BuildFakeOrder(userId: 1, statusName: OrderStatus_Constants.Modification);
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);
        _orderRepo.Setup(x => x.UpdateOrder(1, It.IsAny<Order>(), It.IsAny<List<OHistoryUpdate>>()))
            .ReturnsAsync(existing);

        var input = new UpdateOrderDTO
        {
            OrderName = "Updated",
            Type = "Shirt",
            Color = "Blue",
            StartDate = existing.StartDate,
            EndDate = existing.EndDate,
            Quantity = 5
        };

        var result = await BuildController(userId: 1).UpdateOrder(1, input);

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
        _orderRepo.Setup(x => x.GetOrderDetail(99)).ReturnsAsync((Order)null);

        var result = await BuildController().UpdateOrder(99, new UpdateOrderDTO
        {
            OrderName = "X", Type = "X", Color = "X",
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Quantity = 1
        });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_Returns403_WhenStatusNotModification()
    {
        var existing = BuildFakeOrder(userId: 1, statusName: OrderStatus_Constants.Pending);
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);

        var result = await BuildController(userId: 1).UpdateOrder(1, new UpdateOrderDTO
        {
            OrderName = "X", Type = "X", Color = "X",
            StartDate = existing.StartDate,
            EndDate = existing.EndDate,
            Quantity = 1
        });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_Returns403_WhenUserIsNotOwner()
    {
        var existing = BuildFakeOrder(userId: 99, statusName: OrderStatus_Constants.Modification);
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);

        var result = await BuildController(userId: 1).UpdateOrder(1, new UpdateOrderDTO
        {
            OrderName = "X", Type = "X", Color = "X",
            StartDate = existing.StartDate,
            EndDate = existing.EndDate,
            Quantity = 1
        });

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateOrder_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().UpdateOrder(1, new UpdateOrderDTO
        {
            OrderName = "X", Type = "X", Color = "X",
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Quantity = 1
        });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public async Task RequestOrderModification_Returns200_WhenSuccessful()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Pending);
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);
        _orderRepo.Setup(x => x.RequestOrderModification(1, It.IsAny<Order>(), It.IsAny<List<OHistoryUpdate>>()))
            .ReturnsAsync(existing);

        var result = await BuildController().RequestOrderModification(1);

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task RequestOrderModification_Returns400_WhenIdInvalid()
    {
        var result = await BuildController().RequestOrderModification(0);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task RequestOrderModification_Returns404_WhenOrderNotFound()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(99)).ReturnsAsync((Order)null);

        var result = await BuildController().RequestOrderModification(99);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task RequestOrderModification_Returns403_WhenStatusNotPending()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Approved);
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);

        var result = await BuildController().RequestOrderModification(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task RequestOrderModification_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().RequestOrderModification(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public async Task DenyOrder_Returns200_WhenSuccessful()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Pending);

        _orderRepo.Setup(x => x.GetOrderDetail(It.IsAny<int>()))
            .ReturnsAsync(existing);

        _orderRepo.Setup(x => x.DenyOrder(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Order>(),
            It.IsAny<List<OHistoryUpdate>>()
        )).ReturnsAsync(BuildFakeOrder());

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
        _orderRepo.Setup(x => x.GetOrderDetail(99)).ReturnsAsync((Order)null);

        var result = await BuildController().DenyOrder(99);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task DenyOrder_Returns403_WhenStatusNotPending()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Approved);

        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);

        var result = await BuildController().DenyOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task DenyOrder_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().DenyOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── ApproveOrder ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveOrder_Returns200_WhenSuccessful()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Pending);

        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);
        _orderRepo.Setup(x => x.ApproveOrder(
            It.IsAny<int>(),
            It.IsAny<Order>(),
            It.IsAny<List<OHistoryUpdate>>()
        )).ReturnsAsync(BuildFakeOrder(statusName: OrderStatus_Constants.Approved));

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
        _orderRepo.Setup(x => x.GetOrderDetail(99)).ReturnsAsync((Order)null);

        var result = await BuildController().ApproveOrder(99);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns409_WhenOrderAlreadyApproved()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Approved);
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);

        var result = await BuildController().ApproveOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns409_WhenOrderAlreadyRejected()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Rejected);
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);

        var result = await BuildController().ApproveOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns403_WhenStatusNotPending()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Modification);
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ReturnsAsync(existing);

        var result = await BuildController().ApproveOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveOrder_Returns500_OnException()
    {
        _orderRepo.Setup(x => x.GetOrderDetail(1)).ThrowsAsync(new Exception("db error"));

        var result = await BuildController().ApproveOrder(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }
}
