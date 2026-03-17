using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Moq;

namespace GPMS.TEST.Application.Services;

public class OrderServiceTest
{
    private readonly Mock<IBaseOrderRepositories> _orderBaseRepo = new();
    private readonly Mock<IBaseRepositories<OMaterial>> _materialBaseRepo = new();
    private readonly Mock<IBaseRepositories<User>> _userBaseRepo = new();
    private readonly Mock<IBaseOrderStatusRepositories> _orderStatusRepo = new();

    private OrderService BuildService()
        => new OrderService(
            _orderBaseRepo.Object,
            _materialBaseRepo.Object,
            _userBaseRepo.Object,
            _orderStatusRepo.Object);

    private static Order BuildFakeOrder(
        int id = 1,
        int userId = 1,
        string statusName = OrderStatus_Constants.Pending)
        => new Order
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
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
            Status = 1,
            StatusName = statusName,
            Templates = new List<OTemplate>(),
            Materials = new List<OMaterial>(),
            Histories = new List<OHistoryUpdate>()
        };


    [Fact]
    public async Task GetAllOrders_ReturnsAllOrders_WhenSuccessful()
    {
        var fakeOrders = new List<Order> { BuildFakeOrder(1), BuildFakeOrder(2) };
        _orderBaseRepo.Setup(x => x.GetAll(null)).ReturnsAsync(fakeOrders);

        var service = BuildService();
        var result = await service.GetAllOrders();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllOrders_ReturnsEmpty_WhenNoOrdersExist()
    {
        _orderBaseRepo.Setup(x => x.GetAll(null))
            .ReturnsAsync(new List<Order>());

        var service = BuildService();
        var result = await service.GetAllOrders();

        Assert.Empty(result);
    }


    [Fact]
    public async Task GetOrdersByUserId_ReturnsOrders()
    {
        var fakeOrders = new List<Order>
        {
            BuildFakeOrder(1,5),
            BuildFakeOrder(2,5)
        };

        _orderBaseRepo.Setup(x => x.GetAll(5))
            .ReturnsAsync(fakeOrders);

        var service = BuildService();
        var result = await service.GetOrdersByUserId(5);

        Assert.Equal(2, result.Count());
    }


    [Fact]
    public async Task GetOrderDetail_ReturnsOrder()
    {
        var fakeOrder = BuildFakeOrder();

        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync(fakeOrder);

        var service = BuildService();
        var result = await service.GetOrderDetail(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetOrderDetail_ReturnsNull_WhenNotFound()
    {
        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync((Order)null);

        var service = BuildService();
        var result = await service.GetOrderDetail(1);

        Assert.Null(result);
    }


    [Fact]
    public async Task CreateOrder_ReturnsOrder_WhenSuccessful()
    {
        var order = BuildFakeOrder();
        var created = BuildFakeOrder(id: 10);

        _userBaseRepo.Setup(x => x.GetById(order.UserId))
            .ReturnsAsync(new User { Id = order.UserId });

        _orderBaseRepo.Setup(x => x.Create(order))
            .ReturnsAsync(created);

        var service = BuildService();
        var result = await service.CreateOrder(order);

        Assert.Equal(10, result.Id);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenUserNotFound()
    {
        var order = BuildFakeOrder();

        _userBaseRepo.Setup(x => x.GetById(order.UserId))
            .ReturnsAsync((User)null);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateOrder(order));

        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenEndDateInvalid()
    {
        var order = BuildFakeOrder();
        order.EndDate = order.StartDate.AddDays(-1);

        _userBaseRepo.Setup(x => x.GetById(order.UserId))
            .ReturnsAsync(new User { Id = order.UserId });

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateOrder(order));

        Assert.Equal("End date must be greater than start date.", ex.Message);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenStartDatePast()
    {
        var order = BuildFakeOrder();
        order.StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        _userBaseRepo.Setup(x => x.GetById(order.UserId))
            .ReturnsAsync(new User { Id = order.UserId });

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateOrder(order));

        Assert.Equal("Start date must be greater than current date.", ex.Message);
    }


    [Fact]
    public async Task UpdateOrder_ReturnsUpdatedOrder()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Modification);
        var updated = BuildFakeOrder(statusName: OrderStatus_Constants.Modification);
        updated.OrderName = "Updated";

        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync(existing);

        _orderBaseRepo.Setup(x => x.UpdateOrder(1, updated, It.IsAny<List<OHistoryUpdate>>()))
            .ReturnsAsync(updated);

        var service = BuildService();
        var result = await service.UpdateOrder(1, updated, new List<OHistoryUpdate>());

        Assert.Equal("Updated", result.OrderName);
    }

    [Fact]
    public async Task UpdateOrder_Throws_WhenOrderNotExist()
    {
        var updated = BuildFakeOrder();

        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync((Order)null);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateOrder(1, updated, new()));

        Assert.Equal("Order with id '1' not exist in system.", ex.Message);
    }


    [Fact]
    public async Task UpdateOrder_Throws_WhenEndDateInvalid()
    {
        var updated = BuildFakeOrder(statusName: OrderStatus_Constants.Modification);
        updated.EndDate = updated.StartDate.AddDays(-1);

        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(updated);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateOrder(1, updated, new()));

        Assert.Equal("End date must be greater than start date.", ex.Message);
    }

    [Fact]
    public async Task UpdateOrder_Throws_WhenStatusNotModification()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Pending);
        var updated = BuildFakeOrder(statusName: OrderStatus_Constants.Pending);

        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(existing);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateOrder(1, updated, new()));

        Assert.Contains(OrderStatus_Constants.Modification, ex.Message);
    }

    [Fact]
    public async Task AddMaterial_ReturnsMaterial()
    {
        var order = BuildFakeOrder();
        var material = new OMaterial { Name = "Cotton", Value = 5, Uom = "m" };

        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync(order);

        _materialBaseRepo.Setup(x => x.Create(It.IsAny<OMaterial>()))
            .ReturnsAsync(material);

        var service = BuildService();
        var result = await service.AddMaterial(1, material);

        Assert.Equal(1, result.OrderId);
    }

    [Fact]
    public async Task AddMaterial_Throws_WhenOrderNotFound()
    {
        _orderBaseRepo.Setup(x => x.GetById(99)).ReturnsAsync((Order)null);

        var material = new OMaterial { Name = "Cotton", Value = 5, Uom = "m" };
        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.AddMaterial(99, material));

        Assert.Equal("Order with id '99' not found.", ex.Message);
    }

    [Fact]
    public async Task AddMaterial_Throws_WhenOrderApproved()
    {
        var order = BuildFakeOrder(statusName: OrderStatus_Constants.Approved);
        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(order);

        var material = new OMaterial { Name = "Cotton", Value = 5, Uom = "m" };
        var service = BuildService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddMaterial(1, material));
    }

    [Fact]
    public async Task AddMaterial_Throws_WhenValueInvalid()
    {
        var order = BuildFakeOrder();
        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(order);

        var material = new OMaterial { Name = "Cotton", Value = 0, Uom = "m" };
        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.AddMaterial(1, material));

        Assert.Equal("Quantity must be greater than zero.", ex.Message);
    }


    [Fact]
    public async Task RequestOrderModification_Throws_WhenOrderNotFound()
    {
        var updated = BuildFakeOrder();
        _orderBaseRepo.Setup(x => x.GetById(99)).ReturnsAsync((Order)null);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.RequestOrderModification(99, updated, new()));

        Assert.Equal("Order with id '99' not exist in system.", ex.Message);
    }

    [Fact]
    public async Task RequestOrderModification_ReturnsOrder_WhenSuccessful()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Pending);
        var updated = BuildFakeOrder(statusName: OrderStatus_Constants.Pending);

        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync(existing);

        _orderStatusRepo.Setup(x =>
            x.RequestOrderModification(1, updated, It.IsAny<List<OHistoryUpdate>>()))
            .ReturnsAsync(updated);

        var service = BuildService();
        var result = await service.RequestOrderModification(1, updated, new());

        Assert.NotNull(result);
    }

    [Fact]
    public async Task RequestOrderModification_Throws_WhenStatusInvalid()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Modification);
        var updated = BuildFakeOrder();

        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync(existing);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.RequestOrderModification(1, updated, new()));

        Assert.Equal("Only modify order with status Chờ Xét Duyệt.", ex.Message);
    }

    [Fact]
    public async Task DenyOrder_ReturnsOrder_WhenSuccessful()
    {
        var existing = BuildFakeOrder(userId: 1, statusName: OrderStatus_Constants.Pending);
        var updated = BuildFakeOrder();

        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync(existing);

        _orderStatusRepo.Setup(x => x.DenyOrder(
            1,
            updated,
            It.IsAny<List<OHistoryUpdate>>()))
            .ReturnsAsync(updated);

        var service = BuildService();

        var result = await service.DenyOrder(1, 1, updated, new());

        Assert.NotNull(result);
        Assert.Equal(updated.Id, result.Id);
    }

    [Fact]
    public async Task DenyOrder_Throws_WhenUpdatedOrderNull()
    {
        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.DenyOrder(1, 1, null, new()));

        Assert.Equal("Failed to update order.", ex.Message);
    }

    [Fact]
    public async Task DenyOrder_Throws_WhenOrderNotFound()
    {
        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync((Order)null);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.DenyOrder(1, 1, BuildFakeOrder(), new()));

        Assert.Equal("Order with id '1' not exist in system.", ex.Message);
    }

    [Fact]
    public async Task DenyOrder_Throws_WhenUserNotOwner()
    {
        var existing = BuildFakeOrder(userId: 99); 

        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync(existing);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.DenyOrder(1, 1, BuildFakeOrder(), new()));

        Assert.Equal("You don't have permission to deny this order.", ex.Message);
    }

    [Fact]
    public async Task DenyOrder_Throws_WhenStatusNotPending()
    {
        var existing = BuildFakeOrder(statusName: OrderStatus_Constants.Approved);

        _orderBaseRepo.Setup(x => x.GetById(1))
            .ReturnsAsync(existing);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.DenyOrder(1, 1, BuildFakeOrder(), new()));

        Assert.Equal("Only modify order with status Chờ Xét Duyệt.", ex.Message);
    }
}