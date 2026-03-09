using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Entities;
using Moq;

namespace GPMS.TEST.Application.Services;

public class OrderServiceTest
{
    private readonly Mock<IBaseRepositories<Order>> _orderBaseRepo = new();
    private readonly Mock<IBaseRepositories<OMaterial>> _materialBaseRepo = new();
    private readonly Mock<IBaseRepositories<User>> _userBaseRepo = new();

    private OrderService BuildService() =>
        new OrderService(_orderBaseRepo.Object, _materialBaseRepo.Object, _userBaseRepo.Object);

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

    // ─── GetAllOrders ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllOrders_ReturnsAllOrders_WhenSuccessful()
    {
        var fakeOrders = new List<Order> { BuildFakeOrder(1), BuildFakeOrder(2) };
        _orderBaseRepo.Setup(x => x.GetAll(null)).ReturnsAsync(fakeOrders);

        var service = BuildService();
        var result = await service.GetAllOrders();

        Assert.Equal(2, result.Count());
        _orderBaseRepo.Verify(x => x.GetAll(null), Times.Once);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsEmpty_WhenNoOrdersExist()
    {
        _orderBaseRepo.Setup(x => x.GetAll(null)).ReturnsAsync(new List<Order>());

        var service = BuildService();
        var result = await service.GetAllOrders();

        Assert.Empty(result);
    }

    // ─── GetOrdersByUserId ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrdersByUserId_ReturnsOrders_WhenUserHasOrders()
    {
        var fakeOrders = new List<Order> { BuildFakeOrder(1, userId: 5), BuildFakeOrder(2, userId: 5) };
        _orderBaseRepo.Setup(x => x.GetAll(5)).ReturnsAsync(fakeOrders);

        var service = BuildService();
        var result = await service.GetOrdersByUserId(5);

        Assert.Equal(2, result.Count());
        _orderBaseRepo.Verify(x => x.GetAll(5), Times.Once);
    }

    [Fact]
    public async Task GetOrdersByUserId_ReturnsEmpty_WhenUserHasNoOrders()
    {
        _orderBaseRepo.Setup(x => x.GetAll(99)).ReturnsAsync(new List<Order>());

        var service = BuildService();
        var result = await service.GetOrdersByUserId(99);

        Assert.Empty(result);
    }

    // ─── GetOrderDetail ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrderDetail_ReturnsOrder_WhenOrderExists()
    {
        var fakeOrder = BuildFakeOrder(id: 1);
        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(fakeOrder);

        var service = BuildService();
        var result = await service.GetOrderDetail(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Order", result.OrderName);
    }

    [Fact]
    public async Task GetOrderDetail_ReturnsNull_WhenOrderDoesNotExist()
    {
        _orderBaseRepo.Setup(x => x.GetById(99)).ReturnsAsync((Order)null);

        var service = BuildService();
        var result = await service.GetOrderDetail(99);

        Assert.Null(result);
    }

    // ─── CreateOrder ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrder_ReturnsOrder_WhenSuccessful()
    {
        var order = BuildFakeOrder();
        var createdOrder = BuildFakeOrder(id: 10);
        _userBaseRepo.Setup(x => x.GetById(order.UserId)).ReturnsAsync(new User { Id = order.UserId });
        _orderBaseRepo.Setup(x => x.Create(order)).ReturnsAsync(createdOrder);

        var service = BuildService();
        var result = await service.CreateOrder(order);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        _orderBaseRepo.Verify(x => x.Create(order), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenOrderIsNull()
    {
        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateOrder(null));

        Assert.Equal("Failed to create order.", ex.Message);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenEndDateBeforeStartDate()
    {
        var order = BuildFakeOrder();
        order.StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        order.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateOrder(order));

        Assert.Equal("End date must be greater than start date.", ex.Message);
    }

    // ─── UpdateOrder ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateOrder_ReturnsUpdatedOrder_WhenSuccessful()
    {
        var existing = BuildFakeOrder(id: 1, status: "Modification");
        var updated = BuildFakeOrder(id: 1, status: "Modification");
        updated.OrderName = "Updated Order";

        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(existing);
        _orderBaseRepo.Setup(x => x.Update(updated)).ReturnsAsync(updated);

        var service = BuildService();
        var result = await service.UpdateOrder(1, updated, new List<OHistoryUpdate>());

        Assert.NotNull(result);
        Assert.Equal("Updated Order", result.OrderName);
        _orderBaseRepo.Verify(x => x.Update(updated), Times.Once);
    }

    [Fact]
    public async Task UpdateOrder_ThrowsException_WhenUpdatedOrderIsNull()
    {
        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateOrder(1, null, new List<OHistoryUpdate>()));

        Assert.Equal("Failed to update order.", ex.Message);
    }

    [Fact]
    public async Task UpdateOrder_ThrowsException_WhenEndDateBeforeStartDate()
    {
        var updated = BuildFakeOrder(id: 1, status: "Modification");
        updated.StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        updated.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateOrder(1, updated, new List<OHistoryUpdate>()));

        Assert.Equal("End date must be greater than start date.", ex.Message);
    }

    [Fact]
    public async Task UpdateOrder_ThrowsException_WhenOrderDoesNotExist()
    {
        var updated = BuildFakeOrder(id: 99, status: "Modification");
        _orderBaseRepo.Setup(x => x.GetById(99)).ReturnsAsync((Order)null);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateOrder(99, updated, new List<OHistoryUpdate>()));

        Assert.Equal("Order with id '99' not exist in system.", ex.Message);
    }

    [Fact]
    public async Task UpdateOrder_ThrowsException_WhenOrderStatusIsNotModification()
    {
        var existing = BuildFakeOrder(id: 1, status: "Pending");
        var updated = BuildFakeOrder(id: 1, status: "Pending");
        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(existing);

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateOrder(1, updated, new List<OHistoryUpdate>()));

        Assert.Equal("Only modify order with status 'Modification'.", ex.Message);
    }

    // ─── AddMaterial ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddMaterial_ReturnsMaterial_WhenSuccessful()
    {
        var fakeOrder = BuildFakeOrder(id: 1);
        var material = new OMaterial { Name = "Vải cotton", Value = 10, Uom = "Mét" };
        var createdMaterial = new OMaterial { Id = 5, OrderId = 1, Name = "Vải cotton", Value = 10, Uom = "Mét" };

        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(fakeOrder);
        _materialBaseRepo.Setup(x => x.Create(material)).ReturnsAsync(createdMaterial);

        var service = BuildService();
        var result = await service.AddMaterial(1, material);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal(1, result.OrderId);
        _materialBaseRepo.Verify(x => x.Create(material), Times.Once);
    }

    [Fact]
    public async Task AddMaterial_ThrowsException_WhenOrderDoesNotExist()
    {
        _orderBaseRepo.Setup(x => x.GetById(99)).ReturnsAsync((Order)null);

        var material = new OMaterial { Name = "Vải", Value = 5, Uom = "Mét" };
        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.AddMaterial(99, material));

        Assert.Equal("Order với id '99' không tồn tại trong hệ thống.", ex.Message);
    }

    [Fact]
    public async Task AddMaterial_ThrowsException_WhenValueIsZero()
    {
        var fakeOrder = BuildFakeOrder(id: 1);
        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(fakeOrder);

        var material = new OMaterial { Name = "Vải", Value = 0, Uom = "Mét" };
        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.AddMaterial(1, material));

        Assert.Equal("Quantity must be greater than zero.", ex.Message);
    }

    [Fact]
    public async Task AddMaterial_ThrowsException_WhenValueIsNegative()
    {
        var fakeOrder = BuildFakeOrder(id: 1);
        _orderBaseRepo.Setup(x => x.GetById(1)).ReturnsAsync(fakeOrder);

        var material = new OMaterial { Name = "Vải", Value = -1, Uom = "Mét" };
        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.AddMaterial(1, material));

        Assert.Equal("Quantity must be greater than zero.", ex.Message);
    }

    [Fact]
    public async Task AddMaterial_SetsOrderId_BeforeCreating()
    {
        var fakeOrder = BuildFakeOrder(id: 3);
        var material = new OMaterial { Name = "Vải", Value = 5, Uom = "Mét" };
        _orderBaseRepo.Setup(x => x.GetById(3)).ReturnsAsync(fakeOrder);
        _materialBaseRepo.Setup(x => x.Create(It.IsAny<OMaterial>()))
                         .ReturnsAsync((OMaterial m) => m);

        var service = BuildService();
        var result = await service.AddMaterial(3, material);

        Assert.Equal(3, result.OrderId);
    }
}