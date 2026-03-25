using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Constants
{
    public class CustomLogEvents
    {
        public const int Error_Get = 50001;
        public const int Error_Post = 50002;
        public const int Error_Put = 50003;
        public const int Error_Delete = 50004;
        // ACCOUNT CONTROLLER
        public const int AccountController_Get = 50110;
        public const int AccountController_Post = 50120;
        public const int AccountController_Put = 50130;
        public const int AccountController_Delete = 50140;

        // USER CONTROLLER
        public const int UserController_Get = 50210;
        public const int UserController_Post = 50220;
        public const int UserController_Put = 50230;
        public const int UserController_Delete = 50240;

        //COMMENT CONTROLLER
        public const int CommentController_Get = 50310;
        public const int CommentController_Post = 50320;
        public const int CommentController_Put = 50330;
        public const int CommentController_Delete = 50340;

        //ORDER CONTROLLER
        public const int OrderController_Get = 50410;
        public const int OrderController_Post = 50420;
        public const int OrderController_Put = 50430;
        public const int OrderController_Delete = 50440;

        // LEAVE REQUEST CONTROLLER
        public const int LeaveRequestController_Get = 50510;
        public const int LeaveRequestController_Post = 50520;
        public const int LeaveRequestController_Put = 50530;
        public const int LeaveRequestController_Delete = 50540;

        //WORKER CONTROLLER
        public const int WorkerController_Get = 50610;
        public const int WorkerController_Post = 50620;
        public const int WorkerController_Put = 50630;
        public const int WorkerController_Delete = 50640;


        // PRODUCTION CONTROLLER
        public const int ProductionController_Get = 50710;
        public const int ProductionController_Post = 50720;
        public const int ProductionController_Put = 50730;
        public const int ProductionController_Delete = 50740;
      
        // ORDER REJECT CONTROLLER
        public const int OrderRejectController_Get = 50710;
        public const int OrderRejectController_Post = 50720;
        public const int OrderRejectController_Put = 50730;
        public const int OrderRejectController_Delete = 50740;

        // WORKER ROLE CONTROLLER
        public const int WorkerRoleController_Get = 50810;
        public const int WorkerRoleController_Post = 50820;
        public const int WorkerRoleController_Put = 50830;
        public const int WorkerRoleController_Delete = 50840;

        // PERMISSION CONTROLLER
        public const int PermissionController_Get = 50910;
        public const int PermissionController_Put = 50920;
        //LOG CONTROLLER
        public const int LogController_Get = 51010;
        
    }
}
