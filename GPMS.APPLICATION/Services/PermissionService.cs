using GPMS.APPLICATION.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.Services
{
    public class PermissionService : IPermissionRepositories
    {
        private static readonly Dictionary<string, string> _roleMap = new()
        {
            { "1", "Admin" },
            { "2", "Owner" },
            { "3", "PM" },
            { "4", "Team_Leader" },
            { "5", "Worker" },
            { "6", "KCS" },
            { "7", "Customer" }
        };

        private static readonly List<PermissionEntry> _permissions = new()
        {
            // Account
            new("Account", "Login",    ""),
            new("Account", "Register", ""),

            // User
            new("User", "GetUser",           "1,2"),
            new("User", "GetUserDetail",      "1"),
            new("User", "GetUserList",        "1"),
            new("User", "CreateUser",         "1"),
            new("User", "DisableUser",        "1"),
            new("User", "AssignRoles",        "1"),
            new("User", "UpdateUserForAdmin", "1"),
            new("User", "ViewProfile",        "1,2,3,4,5,6,7"),
            new("User", "UpdateProfile",      "1,2,3,4,5,6,7"),

            // Order
            new("Order", "GetOrders",                "2"),
            new("Order", "GetMyOrders",              "7,2"),
            new("Order", "GetOrderDetail",           "7,2"),
            new("Order", "GetOrderHistory",          "7,2"),
            new("Order", "CreateOrder",              "7,2"),
            new("Order", "AddMaterial",              "7"),
            new("Order", "UpdateOrder",              "7"),
            new("Order", "ApproveOrder",             "2"),
            new("Order", "RequestOrderModification", "2"),
            new("Order", "DenyOrder",                "7"),

            // Order Reject
            new("OrderReject", "CreateOrderReject", "2"),

            // Comment
            new("Comment", "GetComment",    "7,2"),
            new("Comment", "CreateComment", "7,2"),
            new("Comment", "UpdateComment", "7,2"),
            new("Comment", "DeleteComment", "7,2"),

            // Worker
            new("Worker", "GetEmployees",    "2"),
            new("Worker", "GetEmployeeById", "2"),
            new("Worker", "CreateWorker",    "2"),
            new("Worker", "UpdateWorker",    "2"),

            // Worker Role
            new("WorkerRole", "GetAllWorkerRoles", "1,2,3"),
            new("WorkerRole", "CreateWorkerRole",  "1,2,3"),

            // Leave Request
            new("LeaveRequest", "GetLeaveRequests",         "2,3"),
            new("LeaveRequest", "GetMyLeaveRequestHistory", "2,3,4,5"),
            new("LeaveRequest", "GetLeaveRequestDetail",    "2,3,4,5"),
            new("LeaveRequest", "CreateLeaveRequest",       "1,2,3,4,5,6"),
            new("LeaveRequest", "ApproveLeaveRequest",      "2,3"),
            new("LeaveRequest", "DenyLeaveRequest",         "2,3"),
        };

        public Task<IEnumerable<PermissionEntry>> GetAll() => Task.FromResult<IEnumerable<PermissionEntry>>(_permissions);

        public Task<Dictionary<string, string>> GetRoleMap() => Task.FromResult(_roleMap);
    }
}
