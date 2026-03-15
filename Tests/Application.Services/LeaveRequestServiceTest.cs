using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Moq;

namespace GPMS.TEST.Application.Services;

public class LeaveRequestServiceTest
{
    private readonly Mock<IBaseRepositories<LeaveRequest>> _baseRepo = new();

    private LeaveRequestService BuildService()
        => new LeaveRequestService(_baseRepo.Object);

    private static LeaveRequest BuildFakeLeaveRequest(
        int id = 1,
        int userId = 1,
        string statusName = LeaveRequestStatus_Constants.Pending) => new LeaveRequest
        {
            Id = id,
            UserId = userId,
            UserFullName = "Test User",
            Content = "Test leave reason",
            DateCreate = DateTime.UtcNow,
            DateReply = null,
            DenyContent = null,
            StatusId = 1,
            StatusName = statusName
        };

    // ─── GetAllLeaveRequests ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAllLeaveRequests_ReturnsAllLeaveRequests_WhenSuccessful()
    {
        var fakeList = new List<LeaveRequest> { BuildFakeLeaveRequest(1), BuildFakeLeaveRequest(2) };
        _baseRepo.Setup(x => x.GetAll(null)).ReturnsAsync(fakeList);

        var result = await BuildService().GetAllLeaveRequests();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllLeaveRequests_ReturnsEmpty_WhenNoLeaveRequestsExist()
    {
        _baseRepo.Setup(x => x.GetAll(null)).ReturnsAsync(new List<LeaveRequest>());

        var result = await BuildService().GetAllLeaveRequests();

        Assert.Empty(result);
    }

    // ─── GetLeaveRequestsByUserId ────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveRequestsByUserId_ReturnsLeaveRequests_ForGivenUser()
    {
        var fakeList = new List<LeaveRequest> { BuildFakeLeaveRequest(1, userId: 5), BuildFakeLeaveRequest(2, userId: 5) };
        _baseRepo.Setup(x => x.GetAll(5)).ReturnsAsync(fakeList);

        var result = await BuildService().GetLeaveRequestsByUserId(5);

        Assert.Equal(2, result.Count());
        Assert.All(result, lr => Assert.Equal(5, lr.UserId));
    }

    [Fact]
    public async Task GetLeaveRequestsByUserId_ReturnsEmpty_WhenUserHasNoLeaveRequests()
    {
        _baseRepo.Setup(x => x.GetAll(99)).ReturnsAsync(new List<LeaveRequest>());

        var result = await BuildService().GetLeaveRequestsByUserId(99);

        Assert.Empty(result);
    }

    // ─── GetLeaveRequestById ─────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveRequestById_ReturnsLeaveRequest_WhenFound()
    {
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(BuildFakeLeaveRequest(id: 1));

        var result = await BuildService().GetLeaveRequestById(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetLeaveRequestById_ReturnsNull_WhenNotFound()
    {
        _baseRepo.Setup(x => x.GetById(99)).ReturnsAsync((LeaveRequest)null);

        var result = await BuildService().GetLeaveRequestById(99);

        Assert.Null(result);
    }

    // ─── CreateLeaveRequest ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateLeaveRequest_ReturnsCreatedLeaveRequest_WhenSuccessful()
    {
        var expected = BuildFakeLeaveRequest(id: 10, userId: 3);
        _baseRepo.Setup(x => x.Create(It.IsAny<LeaveRequest>())).ReturnsAsync(expected);

        var result = await BuildService().CreateLeaveRequest(userId: 3, content: "Need a day off.");

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal(3, result.UserId);
    }

    [Fact]
    public async Task CreateLeaveRequest_PassesCorrectUserIdToRepo()
    {
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.Create(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        await BuildService().CreateLeaveRequest(userId: 7, content: "Sick day.");

        Assert.NotNull(captured);
        Assert.Equal(7, captured!.UserId);
    }

    [Fact]
    public async Task CreateLeaveRequest_PassesCorrectContentToRepo()
    {
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.Create(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        await BuildService().CreateLeaveRequest(userId: 1, content: "Family emergency.");

        Assert.Equal("Family emergency.", captured!.Content);
    }

    [Fact]
    public async Task CreateLeaveRequest_SetsDateCreateToUtcNow()
    {
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.Create(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        var before = DateTime.UtcNow;
        await BuildService().CreateLeaveRequest(userId: 1, content: "Day off.");
        var after = DateTime.UtcNow;

        Assert.InRange(captured!.DateCreate, before, after);
    }

    // ─── DenyLeaveRequest ────────────────────────────────────────────────────

    [Fact]
    public async Task DenyLeaveRequest_ReturnsDeniedLeaveRequest_WhenSuccessful()
    {
        var pending = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Pending);
        var denied = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Denied);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>())).ReturnsAsync(denied);

        var result = await BuildService().DenyLeaveRequest(1, "Not approved.");

        Assert.NotNull(result);
        Assert.Equal(LeaveRequestStatus_Constants.Denied, result.StatusName);
    }

    [Fact]
    public async Task DenyLeaveRequest_ThrowsKeyNotFoundException_WhenLeaveRequestNotFound()
    {
        _baseRepo.Setup(x => x.GetById(99)).ReturnsAsync((LeaveRequest)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => BuildService().DenyLeaveRequest(99, "Not approved."));

        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task DenyLeaveRequest_ThrowsInvalidOperationException_WhenStatusIsApproved()
    {
        var approved = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Approved);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(approved);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BuildService().DenyLeaveRequest(1, "Not approved."));

        Assert.Contains(LeaveRequestStatus_Constants.Pending, ex.Message);
    }

    [Fact]
    public async Task DenyLeaveRequest_ThrowsInvalidOperationException_WhenStatusIsAlreadyDenied()
    {
        var denied = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Denied);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(denied);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => BuildService().DenyLeaveRequest(1, "Not approved."));
    }

    [Fact]
    public async Task DenyLeaveRequest_SetsStatusIdTo3_BeforeUpdate()
    {
        var pending = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Pending);
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        await BuildService().DenyLeaveRequest(1, "Not approved.");

        Assert.Equal(3, captured!.StatusId);
    }

    [Fact]
    public async Task DenyLeaveRequest_SetsDenyContent_BeforeUpdate()
    {
        var pending = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Pending);
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        await BuildService().DenyLeaveRequest(1, "Budget constraints.");

        Assert.Equal("Budget constraints.", captured!.DenyContent);
    }

    [Fact]
    public async Task DenyLeaveRequest_SetsDateReply_BeforeUpdate()
    {
        var pending = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Pending);
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        var before = DateTime.UtcNow;
        await BuildService().DenyLeaveRequest(1, "Not approved.");
        var after = DateTime.UtcNow;

        Assert.NotNull(captured!.DateReply);
        Assert.InRange(captured.DateReply!.Value, before, after);
    }

    // ─── ApproveLeaveRequest ─────────────────────────────────────────────────

    [Fact]
    public async Task ApproveLeaveRequest_ReturnsApprovedLeaveRequest_WhenSuccessful()
    {
        var pending = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Pending);
        var approved = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Approved);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>())).ReturnsAsync(approved);

        var result = await BuildService().ApproveLeaveRequest(1);

        Assert.NotNull(result);
        Assert.Equal(LeaveRequestStatus_Constants.Approved, result.StatusName);
    }

    [Fact]
    public async Task ApproveLeaveRequest_ThrowsKeyNotFoundException_WhenLeaveRequestNotFound()
    {
        _baseRepo.Setup(x => x.GetById(99)).ReturnsAsync((LeaveRequest)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => BuildService().ApproveLeaveRequest(99));

        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task ApproveLeaveRequest_ThrowsInvalidOperationException_WhenStatusIsAlreadyApproved()
    {
        var approved = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Approved);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(approved);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BuildService().ApproveLeaveRequest(1));

        Assert.Contains(LeaveRequestStatus_Constants.Pending, ex.Message);
    }

    [Fact]
    public async Task ApproveLeaveRequest_ThrowsInvalidOperationException_WhenStatusIsDenied()
    {
        var denied = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Denied);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(denied);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => BuildService().ApproveLeaveRequest(1));
    }

    [Fact]
    public async Task ApproveLeaveRequest_SetsStatusIdTo2_BeforeUpdate()
    {
        var pending = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Pending);
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        await BuildService().ApproveLeaveRequest(1);

        Assert.Equal(2, captured!.StatusId);
    }

    [Fact]
    public async Task ApproveLeaveRequest_SetsDateReply_BeforeUpdate()
    {
        var pending = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Pending);
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        var before = DateTime.UtcNow;
        await BuildService().ApproveLeaveRequest(1);
        var after = DateTime.UtcNow;

        Assert.NotNull(captured!.DateReply);
        Assert.InRange(captured.DateReply!.Value, before, after);
    }
}
