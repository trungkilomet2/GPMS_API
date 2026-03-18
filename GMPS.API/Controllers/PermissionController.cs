using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PermissionController : ControllerBase
    {
        private static readonly List<object> _permissions = new()
        {
            // User
            new { policy = "ManageUsers", roles = new[] { "Admin" }, description = "Quản lý người dùng (tạo, sửa, vô hiệu hóa, phân quyền)" },
            new { policy = "ViewUsers", roles = new[] { "Admin", "Owner" }, description = "Xem danh sách người dùng" },

            // Worker
            new { policy = "ManageWorkers", roles = new[] { "Owner" }, description = "Quản lý nhân viên sản xuất" },
            new { policy = "ManageWorkerRoles", roles = new[] { "Admin", "Owner", "PM" }, description = "Quản lý vai trò nhân viên" },

            // Order
            new { policy = "ManageOrders", roles = new[] { "Owner" }, description = "Xem tất cả đơn hàng, yêu cầu chỉnh sửa" },
            new { policy = "ViewOrders", roles = new[] { "Customer", "Owner" }, description = "Xem đơn hàng, chi tiết đơn hàng" },
            new { policy = "CustomerOrders", roles = new[] { "Customer" }, description = "Thêm vật tư, cập nhật, từ chối đơn hàng" },
            new { policy = "ManageOrderReject", roles = new[] { "Owner" }, description = "Tạo lý do từ chối đơn hàng" },

            // Comment
            new { policy = "ManageComments", roles = new[] { "Customer", "Owner" }, description = "Xem, tạo, sửa, xóa bình luận đơn hàng" },

            // Leave Request
            new { policy = "ManageLeaveRequests", roles = new[] { "Owner", "PM" }, description = "Duyệt hoặc từ chối đơn xin nghỉ" },
            new { policy = "ViewLeaveRequests", roles = new[] { "Owner", "PM", "Team_Leader", "Worker" }, description = "Xem lịch sử đơn xin nghỉ" },
            new { policy = "CreateLeaveRequest", roles = new[] { "Admin", "Owner", "PM", "Team_Leader", "KCS", "Worker" }, description = "Tạo đơn xin nghỉ" },

            // Profile
            new { policy = "AllRoles", roles = new[] { "Admin", "Owner", "PM", "Team_Leader", "Worker", "KCS", "Customer" }, description = "Xem và cập nhật hồ sơ cá nhân" },
        };

        [HttpGet]
        public IActionResult GetPermissions()
        {
            return Ok(_permissions);
        }
    }
}
