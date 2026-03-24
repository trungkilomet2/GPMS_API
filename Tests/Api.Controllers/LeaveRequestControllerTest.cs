using GMPS.API.Controllers;
using GMPS.API.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Enums;
using GPMS.TEST.TestCommon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace GPMS.TEST.Api.Controllers;

public class LeaveRequestControllerTest
{
    private readonly Mock<ILeaveRequestRepositories> _leaveRequestRepo = new();
    private readonly Mock<ILogger<LeaveRequestController>> _logger = new();

    private LeaveRequestController BuildController(int userId = 1, string? role = null)
    {
        var controller = new LeaveRequestController(_leaveRequestRepo.Object, _logger.Object);
        var user = role is not null
            ? BuildUserWithIdAndRole(userId, role)
            : ControllerTestHelper.BuildUserWithId(userId);
        ControllerTestHelper.AttachHttpContext(controller, user);
        return controller;
    }

    private static ClaimsPrincipal BuildUserWithIdAndRole(int id, string role)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Name, "tester"),
            new Claim(ClaimTypes.Role, role)
        ], "TestAuth");

        return new ClaimsPrincipal(identity);
    }

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

    // ─── GetLeaveRequests ────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveRequests_Returns200_WhenSuccessful()
    {
        _leaveRequestRepo.Setup(x => x.GetAllLeaveRequests())
            .ReturnsAsync(new List<LeaveRequest> { BuildFakeLeaveRequest(1), BuildFakeLeaveRequest(2) });

        var result = await BuildController().GetLeaveRequests(new LeaveRequestRequestDTO());

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LeaveRequestListDTO>>>(obj.Value);
        Assert.Equal(2, dto.RecordCount);
    }

    [Fact]
    public async Task GetLeaveRequests_Returns200_WhenListIsEmpty()
    {
        _leaveRequestRepo.Setup(x => x.GetAllLeaveRequests())
            .ReturnsAsync(new List<LeaveRequest>());

        var result = await BuildController().GetLeaveRequests(new LeaveRequestRequestDTO());

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LeaveRequestListDTO>>>(obj.Value);
        Assert.Equal(0, dto.RecordCount);
    }

    [Fact]
    public async Task GetLeaveRequests_Returns400_WhenStatusIsInvalid()
    {
        var input = new LeaveRequestRequestDTO { Status = "InvalidStatus" };

        var result = await BuildController().GetLeaveRequests(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetLeaveRequests_Returns400_WhenDateCreateFromGreaterThanDateCreateTo()
    {
        var input = new LeaveRequestRequestDTO
        {
            DateCreateFrom = DateTime.Today.AddDays(5),
            DateCreateTo = DateTime.Today
        };

        var result = await BuildController().GetLeaveRequests(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetLeaveRequests_Returns404_WhenPageIndexOutOfRange()
    {
        _leaveRequestRepo.Setup(x => x.GetAllLeaveRequests())
            .ReturnsAsync(new List<LeaveRequest> { BuildFakeLeaveRequest() });

        var input = new LeaveRequestRequestDTO { PageIndex = 99, PageSize = 10 };

        var result = await BuildController().GetLeaveRequests(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task GetLeaveRequests_Returns200_WhenFilterByValidStatus()
    {
        _leaveRequestRepo.Setup(x => x.GetAllLeaveRequests())
            .ReturnsAsync(new List<LeaveRequest>
            {
                BuildFakeLeaveRequest(1, statusName: LeaveRequestStatus_Constants.Pending),
                BuildFakeLeaveRequest(2, statusName: LeaveRequestStatus_Constants.Approved)
            });

        var input = new LeaveRequestRequestDTO { Status = LeaveRequestStatus_Constants.Pending };

        var result = await BuildController().GetLeaveRequests(input);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LeaveRequestListDTO>>>(obj.Value);
        Assert.Equal(1, dto.RecordCount);
    }

    [Fact]
    public async Task GetLeaveRequests_Returns200_WhenFilterByLowercaseStatus()
    {
        _leaveRequestRepo.Setup(x => x.GetAllLeaveRequests())
            .ReturnsAsync(new List<LeaveRequest>
            {
                BuildFakeLeaveRequest(1, statusName: LeaveRequestStatus_Constants.Pending),
                BuildFakeLeaveRequest(2, statusName: LeaveRequestStatus_Constants.Approved)
            });

        var input = new LeaveRequestRequestDTO { Status = LeaveRequestStatus_Constants.Pending.ToLower() };

        var result = await BuildController().GetLeaveRequests(input);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LeaveRequestListDTO>>>(obj.Value);
        Assert.Equal(1, dto.RecordCount);
    }

    [Fact]
    public async Task GetLeaveRequests_Returns500_OnException()
    {
        _leaveRequestRepo.Setup(x => x.GetAllLeaveRequests())
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetLeaveRequests(new LeaveRequestRequestDTO());

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── GetMyLeaveRequestHistory ────────────────────────────────────────────

    [Fact]
    public async Task GetMyLeaveRequestHistory_Returns200_WhenSuccessful()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestsByUserId(1))
            .ReturnsAsync(new List<LeaveRequest> { BuildFakeLeaveRequest(1), BuildFakeLeaveRequest(2) });

        var result = await BuildController(userId: 1).GetMyLeaveRequestHistory(new LeaveRequestRequestDTO());

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LeaveRequestListDTO>>>(obj.Value);
        Assert.Equal(2, dto.RecordCount);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistory_Returns200_WhenListIsEmpty()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestsByUserId(1))
            .ReturnsAsync(new List<LeaveRequest>());

        var result = await BuildController(userId: 1).GetMyLeaveRequestHistory(new LeaveRequestRequestDTO());

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LeaveRequestListDTO>>>(obj.Value);
        Assert.Equal(0, dto.RecordCount);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistory_Returns400_WhenStatusIsInvalid()
    {
        var input = new LeaveRequestRequestDTO { Status = "BadValue" };

        var result = await BuildController().GetMyLeaveRequestHistory(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistory_Returns400_WhenDateCreateFromGreaterThanDateCreateTo()
    {
        var input = new LeaveRequestRequestDTO
        {
            DateCreateFrom = DateTime.Today.AddDays(3),
            DateCreateTo = DateTime.Today
        };

        var result = await BuildController().GetMyLeaveRequestHistory(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistory_Returns404_WhenPageIndexOutOfRange()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestsByUserId(1))
            .ReturnsAsync(new List<LeaveRequest> { BuildFakeLeaveRequest() });

        var input = new LeaveRequestRequestDTO { PageIndex = 99, PageSize = 10 };

        var result = await BuildController(userId: 1).GetMyLeaveRequestHistory(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistory_Returns200_WhenFilterByValidStatus()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestsByUserId(1))
            .ReturnsAsync(new List<LeaveRequest>
            {
                BuildFakeLeaveRequest(1, statusName: LeaveRequestStatus_Constants.Approved),
                BuildFakeLeaveRequest(2, statusName: LeaveRequestStatus_Constants.Denied)
            });

        var input = new LeaveRequestRequestDTO { Status = LeaveRequestStatus_Constants.Approved };

        var result = await BuildController(userId: 1).GetMyLeaveRequestHistory(input);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LeaveRequestListDTO>>>(obj.Value);
        Assert.Equal(1, dto.RecordCount);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistory_Returns200_WhenFilterByLowercaseStatus()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestsByUserId(1))
            .ReturnsAsync(new List<LeaveRequest>
            {
                BuildFakeLeaveRequest(1, statusName: LeaveRequestStatus_Constants.Approved),
                BuildFakeLeaveRequest(2, statusName: LeaveRequestStatus_Constants.Denied)
            });

        var input = new LeaveRequestRequestDTO { Status = LeaveRequestStatus_Constants.Approved.ToLower() };

        var result = await BuildController(userId: 1).GetMyLeaveRequestHistory(input);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<IEnumerable<LeaveRequestListDTO>>>(obj.Value);
        Assert.Equal(1, dto.RecordCount);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistory_Returns500_OnException()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestsByUserId(It.IsAny<int>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().GetMyLeaveRequestHistory(new LeaveRequestRequestDTO());

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── GetMyLeaveRequestHistoryDetail ──────────────────────────────────────

    [Fact]
    public async Task GetMyLeaveRequestHistoryDetail_Returns200_WhenFound()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestById(1))
            .ReturnsAsync(BuildFakeLeaveRequest(id: 1, userId: 1));

        var result = await BuildController(userId: 1).GetMyLeaveRequestHistoryDetail(1);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<LeaveRequestDetailDTO>>(obj.Value);
        Assert.Equal(1, dto.Data.Id);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistoryDetail_Returns400_WhenIdInvalid()
    {
        var result = await BuildController().GetMyLeaveRequestHistoryDetail(0);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistoryDetail_Returns400_WhenIdNegative()
    {
        var result = await BuildController().GetMyLeaveRequestHistoryDetail(-5);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistoryDetail_Returns404_WhenNotFound()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestById(99))
            .ReturnsAsync((LeaveRequest)null);

        var result = await BuildController(userId: 1).GetMyLeaveRequestHistoryDetail(99);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistoryDetail_Returns403_WhenLeaveRequestBelongsToOtherUser()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestById(1))
            .ReturnsAsync(BuildFakeLeaveRequest(id: 1, userId: 99));

        var result = await BuildController(userId: 1).GetMyLeaveRequestHistoryDetail(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task GetMyLeaveRequestHistoryDetail_Returns500_OnException()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestById(1))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController(userId: 1).GetMyLeaveRequestHistoryDetail(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── GetLeaveRequestDetail ───────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveRequestDetail_Returns200_WhenOwnerAccessesAnyLeaveRequest()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestById(1))
            .ReturnsAsync(BuildFakeLeaveRequest(id: 1, userId: 99));

        var result = await BuildController(userId: 1, role: nameof(RoleName.Owner)).GetLeaveRequestDetail(1);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<LeaveRequestDetailDTO>>(obj.Value);
        Assert.Equal(1, dto.Data.Id);
    }

    [Fact]
    public async Task GetLeaveRequestDetail_Returns200_WhenPMAccessesAnyLeaveRequest()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestById(1))
            .ReturnsAsync(BuildFakeLeaveRequest(id: 1, userId: 99));

        var result = await BuildController(userId: 1, role: nameof(RoleName.PM)).GetLeaveRequestDetail(1);

        var obj = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RestDTO<LeaveRequestDetailDTO>>(obj.Value);
        Assert.Equal(1, dto.Data.Id);
    }

    [Fact]
    public async Task GetLeaveRequestDetail_Returns400_WhenIdInvalid()
    {
        var result = await BuildController(role: nameof(RoleName.Owner)).GetLeaveRequestDetail(0);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task GetLeaveRequestDetail_Returns404_WhenNotFound()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestById(99))
            .ReturnsAsync((LeaveRequest)null);

        var result = await BuildController(userId: 1, role: nameof(RoleName.Owner)).GetLeaveRequestDetail(99);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task GetLeaveRequestDetail_Returns500_OnException()
    {
        _leaveRequestRepo.Setup(x => x.GetLeaveRequestById(1))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController(userId: 1, role: nameof(RoleName.Owner)).GetLeaveRequestDetail(1);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── CreateLeaveRequest ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateLeaveRequest_Returns201_WhenSuccessful()
    {
        _leaveRequestRepo.Setup(x => x.CreateLeaveRequest(1, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(BuildFakeLeaveRequest(id: 5, userId: 1));

        var input = new CreateLeaveRequestDTO { Content = "I need a day off." };

        var result = await BuildController(userId: 1).CreateLeaveRequest(input);

        var obj = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, obj.StatusCode);
    }

    [Fact]
    public async Task CreateLeaveRequest_Returns400_WhenModelStateInvalid()
    {
        var controller = BuildController(userId: 1);
        controller.ModelState.AddModelError("Content", "Content is required.");

        var result = await controller.CreateLeaveRequest(new CreateLeaveRequestDTO { Content = "" });

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task CreateLeaveRequest_Returns500_WhenPendingStatusNotFoundInSystem()
    {
        _leaveRequestRepo.Setup(x => x.CreateLeaveRequest(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new InvalidOperationException("Status 'Pending' not found in system."));

        var input = new CreateLeaveRequestDTO { Content = "I need a day off." };

        var result = await BuildController(userId: 1).CreateLeaveRequest(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public async Task CreateLeaveRequest_Returns500_OnException()
    {
        _leaveRequestRepo.Setup(x => x.CreateLeaveRequest(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new CreateLeaveRequestDTO { Content = "I need a day off." };

        var result = await BuildController(userId: 1).CreateLeaveRequest(input);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── DenyLeaveRequest ────────────────────────────────────────────────────

    [Fact]
    public async Task DenyLeaveRequest_Returns200_WhenSuccessful()
    {
        _leaveRequestRepo.Setup(x => x.DenyLeaveRequest(1, It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Denied));

        var input = new DenyLeaveRequestDTO { DenyContent = "Not approved." };

        var result = await BuildController().DenyLeaveRequest(1, input);

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task DenyLeaveRequest_Returns400_WhenIdInvalid()
    {
        var input = new DenyLeaveRequestDTO { DenyContent = "Not approved." };

        var result = await BuildController().DenyLeaveRequest(0, input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task DenyLeaveRequest_Returns400_WhenModelStateInvalid()
    {
        var controller = BuildController();
        controller.ModelState.AddModelError("DenyContent", "Deny reason is required.");

        var result = await controller.DenyLeaveRequest(1, new DenyLeaveRequestDTO { DenyContent = "" });

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task DenyLeaveRequest_Returns404_WhenLeaveRequestNotFound()
    {
        _leaveRequestRepo.Setup(x => x.DenyLeaveRequest(99, It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException("Leave request with id '99' not found."));

        var input = new DenyLeaveRequestDTO { DenyContent = "Not approved." };

        var result = await BuildController().DenyLeaveRequest(99, input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task DenyLeaveRequest_Returns403_WhenStatusIsNotPending()
    {
        _leaveRequestRepo.Setup(x => x.DenyLeaveRequest(1, It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Only leave requests with status 'Pending' can be denied."));

        var input = new DenyLeaveRequestDTO { DenyContent = "Not approved." };

        var result = await BuildController().DenyLeaveRequest(1, input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task DenyLeaveRequest_Returns500_OnException()
    {
        _leaveRequestRepo.Setup(x => x.DenyLeaveRequest(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("db error"));

        var input = new DenyLeaveRequestDTO { DenyContent = "Not approved." };

        var result = await BuildController().DenyLeaveRequest(1, input);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // ─── ApproveLeaveRequest ─────────────────────────────────────────────────

    [Fact]
    public async Task ApproveLeaveRequest_Returns200_WhenSuccessful()
    {
        _leaveRequestRepo.Setup(x => x.ApproveLeaveRequest(1, It.IsAny<int>()))
            .ReturnsAsync(BuildFakeLeaveRequest(statusName: LeaveRequestStatus_Constants.Approved));

        var result = await BuildController().ApproveLeaveRequest(1);

        var obj = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveLeaveRequest_Returns400_WhenIdInvalid()
    {
        var result = await BuildController().ApproveLeaveRequest(0);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveLeaveRequest_Returns400_WhenIdNegative()
    {
        var result = await BuildController().ApproveLeaveRequest(-1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveLeaveRequest_Returns404_WhenLeaveRequestNotFound()
    {
        _leaveRequestRepo.Setup(x => x.ApproveLeaveRequest(99, It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException("Leave request with id '99' not found."));

        var result = await BuildController().ApproveLeaveRequest(99);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveLeaveRequest_Returns403_WhenStatusIsNotPending()
    {
        _leaveRequestRepo.Setup(x => x.ApproveLeaveRequest(1, It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Only leave requests with status 'Pending' can be approved."));

        var result = await BuildController().ApproveLeaveRequest(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task ApproveLeaveRequest_Returns500_OnException()
    {
        _leaveRequestRepo.Setup(x => x.ApproveLeaveRequest(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await BuildController().ApproveLeaveRequest(1);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }
}
