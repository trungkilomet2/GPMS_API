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
            StatusId = LeaveRequestStatus_Constants.Pending_ID,
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

        var result = await BuildService().CreateLeaveRequest(userId: 3, content: "Need a day off.", fromDate: null, toDate: null);

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

        await BuildService().CreateLeaveRequest(userId: 7, content: "Sick day.", fromDate: null, toDate: null);

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

        await BuildService().CreateLeaveRequest(userId: 1, content: "Family emergency.", fromDate: null, toDate: null);

        Assert.Equal("Family emergency.", captured!.Content);
    }

    [Fact]
    public async Task CreateLeaveRequest_ThrowsInvalidOperationException_WhenFromDateIsInThePast()
    {
        var yesterday = DateTime.Today.AddDays(-1);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            BuildService().CreateLeaveRequest(userId: 1, content: "Day off.", fromDate: yesterday, toDate: null));
    }

    [Fact]
    public async Task CreateLeaveRequest_ThrowsInvalidOperationException_WhenToDateIsToday()
    {
        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).Date;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            BuildService().CreateLeaveRequest(userId: 1, content: "Day off.", fromDate: null, toDate: today));
    }

    [Fact]
    public async Task CreateLeaveRequest_ThrowsInvalidOperationException_WhenToDateIsInThePast()
    {
        var yesterday = DateTime.Today.AddDays(-1);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            BuildService().CreateLeaveRequest(userId: 1, content: "Day off.", fromDate: null, toDate: yesterday));
    }

    [Fact]
    public async Task CreateLeaveRequest_ThrowsInvalidOperationException_WhenToDateEqualsFromDate()
    {
        var fromDate = DateTime.Today.AddDays(2);
        var toDate = DateTime.Today.AddDays(2);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            BuildService().CreateLeaveRequest(userId: 1, content: "Day off.", fromDate: fromDate, toDate: toDate));
    }

    [Fact]
    public async Task CreateLeaveRequest_ThrowsInvalidOperationException_WhenToDateIsBeforeFromDate()
    {
        var fromDate = DateTime.Today.AddDays(3);
        var toDate = DateTime.Today.AddDays(2);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            BuildService().CreateLeaveRequest(userId: 1, content: "Day off.", fromDate: fromDate, toDate: toDate));
    }

    [Fact]
    public async Task CreateLeaveRequest_SetsDateCreateToVietnamTime()
    {
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.Create(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var before = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        await BuildService().CreateLeaveRequest(userId: 1, content: "Day off.", fromDate: null, toDate: null);
        var after = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

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

        var result = await BuildService().DenyLeaveRequest(1, "Not approved.", approverId: 10);

        Assert.NotNull(result);
        Assert.Equal(LeaveRequestStatus_Constants.Denied, result.StatusName);
    }

    [Fact]
    public async Task DenyLeaveRequest_ThrowsKeyNotFoundException_WhenLeaveRequestNotFound()
    {
        _baseRepo.Setup(x => x.GetById(99)).ReturnsAsync((LeaveRequest)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => BuildService().DenyLeaveRequest(99, "Not approved.", approverId: 10));

        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task DenyLeaveRequest_ThrowsInvalidOperationException_WhenStatusIsApproved()
    {
        var approved = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Approved);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(approved);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BuildService().DenyLeaveRequest(1, "Not approved.", approverId: 10));

        Assert.Contains(LeaveRequestStatus_Constants.Pending, ex.Message);
    }

    [Fact]
    public async Task DenyLeaveRequest_ThrowsInvalidOperationException_WhenStatusIsAlreadyDenied()
    {
        var denied = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Denied);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(denied);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => BuildService().DenyLeaveRequest(1, "Not approved.", approverId: 10));
    }

    [Fact]
    public async Task DenyLeaveRequest_SetsStatusIdToDeniedId_BeforeUpdate()
    {
        var pending = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Pending);
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        await BuildService().DenyLeaveRequest(1, "Not approved.", approverId: 10);

        Assert.Equal(LeaveRequestStatus_Constants.Denied_ID, captured!.StatusId);
        Assert.Equal(10, captured!.ApprovedBy);
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

        await BuildService().DenyLeaveRequest(1, "Budget constraints.", approverId: 10);

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

        var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var before = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        await BuildService().DenyLeaveRequest(1, "Not approved.", approverId: 10);
        var after = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

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

        var result = await BuildService().ApproveLeaveRequest(1, approverId: 10);

        Assert.NotNull(result);
        Assert.Equal(LeaveRequestStatus_Constants.Approved, result.StatusName);
    }

    [Fact]
    public async Task ApproveLeaveRequest_ThrowsKeyNotFoundException_WhenLeaveRequestNotFound()
    {
        _baseRepo.Setup(x => x.GetById(99)).ReturnsAsync((LeaveRequest)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => BuildService().ApproveLeaveRequest(99, approverId: 10));

        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task ApproveLeaveRequest_ThrowsInvalidOperationException_WhenStatusIsAlreadyApproved()
    {
        var approved = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Approved);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(approved);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BuildService().ApproveLeaveRequest(1, approverId: 10));

        Assert.Contains(LeaveRequestStatus_Constants.Pending, ex.Message);
    }

    [Fact]
    public async Task ApproveLeaveRequest_ThrowsInvalidOperationException_WhenStatusIsDenied()
    {
        var denied = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Denied);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(denied);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => BuildService().ApproveLeaveRequest(1, approverId: 10));
    }

    [Fact]
    public async Task ApproveLeaveRequest_SetsStatusIdToApprovedId_BeforeUpdate()
    {
        var pending = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Pending);
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        await BuildService().ApproveLeaveRequest(1, approverId: 10);

        Assert.Equal(LeaveRequestStatus_Constants.Approved_ID, captured!.StatusId);
        Assert.Equal(10, captured!.ApprovedBy);
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

        var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var before = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        await BuildService().ApproveLeaveRequest(1, approverId: 10);
        var after = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

        Assert.NotNull(captured!.DateReply);
        Assert.InRange(captured.DateReply!.Value, before, after);
    }

    // ─── CancelLeaveRequest ──────────────────────────────────────────────────

    [Fact]
    public async Task CancelLeaveRequest_ReturnsCancelledLeaveRequest_WhenSuccessful()
    {
        var pending = BuildFakeLeaveRequest(userId: 1, statusName: LeaveRequestStatus_Constants.Pending);
        var cancelled = BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Cancelled);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>())).ReturnsAsync(cancelled);

        var result = await BuildService().CancelLeaveRequest(1, userId: 1);

        Assert.NotNull(result);
        Assert.Equal(LeaveRequestStatus_Constants.Cancelled, result.StatusName);
    }

    [Fact]
    public async Task CancelLeaveRequest_ThrowsKeyNotFoundException_WhenNotFound()
    {
        _baseRepo.Setup(x => x.GetById(99)).ReturnsAsync((LeaveRequest)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => BuildService().CancelLeaveRequest(99, userId: 1));

        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task CancelLeaveRequest_ThrowsUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        var pending = BuildFakeLeaveRequest(userId: 5, statusName: LeaveRequestStatus_Constants.Pending);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => BuildService().CancelLeaveRequest(1, userId: 99));
    }

    [Fact]
    public async Task CancelLeaveRequest_ThrowsInvalidOperationException_WhenStatusIsApproved()
    {
        var approved = BuildFakeLeaveRequest(userId: 1, statusName: LeaveRequestStatus_Constants.Approved);
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(approved);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BuildService().CancelLeaveRequest(1, userId: 1));

        Assert.Contains(LeaveRequestStatus_Constants.Pending, ex.Message);
    }

    [Fact]
    public async Task CancelLeaveRequest_SetsStatusIdToCancelledId_BeforeUpdate()
    {
        var pending = BuildFakeLeaveRequest(userId: 1, statusName: LeaveRequestStatus_Constants.Pending);
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        await BuildService().CancelLeaveRequest(1, userId: 1);

        Assert.Equal(LeaveRequestStatus_Constants.Cancelled_ID, captured!.StatusId);
    }

    [Fact]
    public async Task CancelLeaveRequest_SetsDateReply_BeforeUpdate()
    {
        var pending = BuildFakeLeaveRequest(userId: 1, statusName: LeaveRequestStatus_Constants.Pending);
        LeaveRequest? captured = null;
        _baseRepo.Setup(x => x.GetById(1)).ReturnsAsync(pending);
        _baseRepo.Setup(x => x.Update(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(lr => captured = lr)
            .ReturnsAsync(BuildFakeLeaveRequest());

        var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var before = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        await BuildService().CancelLeaveRequest(1, userId: 1);
        var after = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

        Assert.NotNull(captured!.DateReply);
        Assert.InRange(captured.DateReply!.Value, before, after);
    }
}
