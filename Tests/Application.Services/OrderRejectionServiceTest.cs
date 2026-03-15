using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Moq;

namespace GPMS.TEST.Application.Services;

public class OrderRejectServiceTest
{
    private readonly Mock<IBaseRepositories<OrderRejectReason>> _rejectRepo = new();
    private readonly Mock<IBaseRepositories<Order>> _orderRepo = new();
    private readonly Mock<IBaseOrderRepositories> _baseOrderRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private OrderRejectService BuildService()
        => new OrderRejectService(
            _rejectRepo.Object,
            _unitOfWork.Object,
            _orderRepo.Object,
            _baseOrderRepo.Object);

    private static Order FakeOrder(
        int id = 1,
        string status = OrderStatus_Constants.Pending)
        => new Order
        {
            Id = id,
            Status = 1,
            StatusName = status
        };

    private static OrderRejectReason FakeReason(int orderId = 1)
        => new OrderRejectReason
        {
            Id = 1,
            OrderId = orderId,
            Reason = "Wrong design"
        };

    private void SetupTransaction()
    {
        _unitOfWork.Setup(x =>
            x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>((action, _) => action());
    }

    [Fact]
    public async Task CreateReason_Success()
    {
        var order = FakeOrder();
        var reason = FakeReason();

        _orderRepo.Setup(x => x.GetById(reason.OrderId))
            .ReturnsAsync(order);

        SetupTransaction();

        var service = BuildService();

        var result = await service.CreateReason(reason);

        Assert.NotNull(result);

        _baseOrderRepo.Verify(x =>
            x.ChangeStatus(reason.OrderId, 4), Times.Once);

        _rejectRepo.Verify(x =>
            x.Create(reason), Times.Once);

        _unitOfWork.Verify(x =>
            x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }


    [Fact]
    public async Task CreateReason_Throws_WhenOrderNotFound()
    {
        var reason = FakeReason();

        _orderRepo.Setup(x => x.GetById(reason.OrderId))
            .ReturnsAsync((Order)null);

        SetupTransaction();

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateReason(reason));

        Assert.Equal($"Order with ID {reason.OrderId} does not exist.", ex.Message);
    }


    [Fact]
    public async Task CreateReason_Throws_WhenStatusNotPending()
    {
        var order = FakeOrder(status: OrderStatus_Constants.Approved);
        var reason = FakeReason();

        _orderRepo.Setup(x => x.GetById(reason.OrderId))
            .ReturnsAsync(order);

        SetupTransaction();

        var service = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateReason(reason));

        Assert.Equal(
            $"Order with ID {reason.OrderId} is not in a pending state and cannot be rejected.",
            ex.Message);
    }


    [Fact]
    public async Task CreateReason_ChangesStatusCorrectly()
    {
        var order = FakeOrder();
        var reason = FakeReason();

        _orderRepo.Setup(x => x.GetById(reason.OrderId))
            .ReturnsAsync(order);

        SetupTransaction();

        var service = BuildService();

        await service.CreateReason(reason);

        _baseOrderRepo.Verify(x =>
            x.ChangeStatus(reason.OrderId, 4),
            Times.Once);
    }
}