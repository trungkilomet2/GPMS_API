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
            new("Account", "POST", "Login",    ""),
            new("Account", "POST", "Register", ""),

            // User
            new("User", "GET",  "GetUser",           "1,2"),
            new("User", "GET",  "GetUserDetail",      "1"),
            new("User", "GET",  "GetUserList",        "1"),
            new("User", "POST", "CreateUser",         "1"),
            new("User", "PUT",  "DisableUser",        "1"),
            new("User", "PUT",  "AssignRoles",        "1"),
            new("User", "PUT",  "UpdateUserForAdmin", "1"),
            new("User", "GET",  "ViewProfile",        "1,2,3,4,5,6,7"),
            new("User", "PUT",  "UpdateProfile",      "1,2,3,4,5,6,7"),

            // Order
            new("Order", "GET",  "GetOrders",                "2"),
            new("Order", "GET",  "GetMyOrders",              "7,2"),
            new("Order", "GET",  "GetOrderDetail",           "7,2"),
            new("Order", "GET",  "GetOrderHistory",          "7,2"),
            new("Order", "POST", "CreateOrder",              "7,2"),
            new("Order", "POST", "AddMaterial",              "7"),
            new("Order", "PUT",  "UpdateOrder",              "7"),
            new("Order", "PUT",  "ApproveOrder",             "2"),
            new("Order", "PUT",  "RequestOrderModification", "2"),
            new("Order", "PUT",  "DenyOrder",                "7"),

            // Order Reject
            new("OrderReject", "POST", "CreateOrderReject", "2"),

            // Comment
            new("Comment", "GET",    "GetComment",    "7,2"),
            new("Comment", "POST",   "CreateComment", "7,2"),
            new("Comment", "PUT",    "UpdateComment", "7,2"),
            new("Comment", "DELETE", "DeleteComment", "7,2"),

            // Worker
            new("Worker", "GET",  "GetEmployees",    "2"),
            new("Worker", "GET",  "GetEmployeeById", "2"),
            new("Worker", "POST", "CreateWorker",    "2"),
            new("Worker", "PUT",  "UpdateWorker",    "2"),

            // Worker Role
            new("WorkerRole", "GET",  "GetAllWorkerRoles", "1,2,3"),
            new("WorkerRole", "POST", "CreateWorkerRole",  "1,2,3"),

            // Leave Request
            new("LeaveRequest", "GET",  "GetLeaveRequests",         "2,3"),
            new("LeaveRequest", "GET",  "GetMyLeaveRequestHistory", "2,3,4,5"),
            new("LeaveRequest", "GET",  "GetLeaveRequestDetail",    "2,3,4,5"),
            new("LeaveRequest", "POST", "CreateLeaveRequest",       "1,2,3,4,5,6"),
            new("LeaveRequest", "PUT",  "ApproveLeaveRequest",      "2,3"),
            new("LeaveRequest", "PUT",  "DenyLeaveRequest",         "2,3"),
        };

        public Task<IEnumerable<PermissionEntry>> GetAll() => Task.FromResult<IEnumerable<PermissionEntry>>(_permissions);

        public Task<Dictionary<string, string>> GetRoleMap() => Task.FromResult(_roleMap);
    }
}
