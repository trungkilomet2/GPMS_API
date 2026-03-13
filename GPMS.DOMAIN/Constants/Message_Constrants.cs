using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Constants
{
    public static class MessageCodes
    {
        // Validation
        public const string VAL_NO_SEARCH_RESULT = "VAL-001";
        public const string VAL_REQUIRED_FIELD = "VAL-002";
        public const string VAL_INVALID_FORMAT = "VAL-003";
        // System
        public const string SYS_ACTION_FAILED = "SYS-001";
        // Authentication
        public const string AUTH_LOGIN_SUCCESS = "AUTH-001";
        public const string AUTH_LOGIN_FAILED = "AUTH-002";
        public const string AUTH_LOGOUT_SUCCESS = "AUTH-003";
        public const string AUTH_REGISTER_SUCCESS = "AUTH-004";
        public const string AUTH_PERMISSION_DENIED = "AUTH-005";
        // User
        public const string USER_PROFILE_UPDATED = "USER-001";
        // Order
        public const string ORD_CREATE_SUCCESS = "ORD-001";
        public const string ORD_UPDATE_SUCCESS = "ORD-002";
        public const string ORD_CANCEL_SUCCESS = "ORD-003";
        public const string ORD_STATUS_UPDATED = "ORD-004";
        public const string ORD_INVALID_STATUS = "ORD-005";
        public const string ORD_REVISION_REQUESTED = "ORD-006";
        public const string ORD_COMPLETED = "ORD-007";
        // Comment
        public const string CMT_ADD_SUCCESS = "CMT-001";
        public const string CMT_EDIT_SUCCESS = "CMT-002";
        public const string CMT_EDIT_DENIED = "CMT-003";
        // Production
        public const string PROD_PLAN_CREATED = "PROD-001";
        public const string PROD_PLAN_ACCEPTED = "PROD-002";
        public const string PROD_PLAN_REJECTED = "PROD-003";
        public const string PROD_STAGE_ADDED = "PROD-004";
        public const string PROD_STAGE_UPDATED = "PROD-005";
        public const string PROD_STAGE_DELETED = "PROD-006";
        // Task
        public const string TASK_ASSIGNED = "TASK-001";
        public const string TASK_STATUS_UPDATED = "TASK-002";
        public const string TASK_OUTPUT_SUBMITTED = "TASK-003";
        // Quality
        public const string QA_INSPECTION_RECORDED = "QA-001";
        public const string QA_APPROVED = "QA-002";
        public const string QA_REJECTED = "QA-003";
        public const string QA_REWORK_ASSIGNED = "QA-004";
        // Employee
        public const string EMP_CREATED = "EMP-001";
        public const string EMP_UPDATED = "EMP-002";
        public const string EMP_DISABLED = "EMP-003";
        // Leave
        public const string LEAVE_SUBMITTED = "LEAVE-001";
        public const string LEAVE_APPROVED = "LEAVE-002";
        public const string LEAVE_REJECTED = "LEAVE-003";
        // Payroll
        public const string PAY_STATUS_UPDATED = "PAY-001";
        public const string PAY_EXPORTED = "PAY-002";
        // Report
        public const string REP_NO_DATA = "REP-001";
        // Notification
        public const string NOTI_SENT = "NOTI-001";
        public const string NOTI_FAILED = "NOTI-002";
    }


    public static class MessagesContents
    {
        public const string NO_SEARCH_RESULT = "No search results found.";
        public const string REQUIRED_FIELD = "This field is required.";
        public const string INVALID_FORMAT = "Invalid input format.";

        public const string ACTION_FAILED = "An error occurred. Please try again.";

        public const string LOGIN_SUCCESS = "Logged in successfully.";
        public const string LOGIN_FAILED = "Invalid username or password.";
        public const string LOGOUT_SUCCESS = "Logged out successfully.";
        public const string REGISTER_SUCCESS = "Account created successfully.";
        public const string PERMISSION_DENIED = "You do not have permission to perform this action.";

        public const string PROFILE_UPDATED = "Profile updated successfully.";

        public const string ORDER_CREATED = "Created order successfully.";
        public const string ORDER_UPDATED = "Updated order successfully.";
        public const string ORDER_CANCELLED = "Order cancelled successfully.";
        public const string ORDER_STATUS_UPDATED = "Updated order status successfully.";
        public const string ORDER_INVALID_STATUS = "This order cannot be modified in its current status.";
        public const string ORDER_REVISION = "Revision request sent successfully.";
        public const string ORDER_COMPLETED = "Order completed successfully.";

        public const string COMMENT_ADDED = "Comment posted successfully.";
        public const string COMMENT_UPDATED = "Comment updated successfully.";
        public const string COMMENT_EDIT_DENIED = "You cannot edit this comment.";

        public const string PRODUCTION_PLAN_CREATED = "Production plan created successfully.";
        public const string PRODUCTION_PLAN_ACCEPTED = "Production plan accepted.";
        public const string PRODUCTION_PLAN_REJECTED = "Production plan rejected.";
        public const string PRODUCTION_STAGE_ADDED = "Production stage added successfully.";
        public const string PRODUCTION_STAGE_UPDATED = "Production stage updated successfully.";
        public const string PRODUCTION_STAGE_DELETED = "Production stage deleted successfully.";

        public const string TASK_ASSIGNED = "Task assigned successfully.";
        public const string TASK_STATUS_UPDATED = "Task status updated successfully.";
        public const string DAILY_OUTPUT_SUBMITTED = "Daily output submitted successfully.";

        public const string INSPECTION_RECORDED = "Inspection result recorded successfully.";
        public const string QUALITY_APPROVED = "Product quality approved.";
        public const string QUALITY_REJECTED = "Product quality rejected.";
        public const string PRODUCT_REWORK = "Product reassigned for rework.";

        public const string EMPLOYEE_CREATED = "Employee created successfully.";
        public const string EMPLOYEE_UPDATED = "Employee updated successfully.";
        public const string EMPLOYEE_DISABLED = "Employee account disabled successfully.";

        public const string LEAVE_SUBMITTED = "Leave request submitted successfully.";
        public const string LEAVE_APPROVED = "Leave request approved.";
        public const string LEAVE_REJECTED = "Leave request rejected.";

        public const string PAYROLL_UPDATED = "Payroll status updated successfully.";
        public const string PAYROLL_EXPORTED = "Payroll exported to Excel successfully.";

        public const string NO_REPORT_DATA = "No data available for this report.";

        public const string NOTIFICATION_SENT = "Notification sent successfully.";
        public const string NOTIFICATION_FAILED = "Failed to send notification.";
    }




}
