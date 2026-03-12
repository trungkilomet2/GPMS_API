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

        //WORKER CONTROLLER
        public const int WorkerController_Get = 50510;
        public const int WorkerController_Post = 50520;
        public const int WorkerController_Put = 50530;
        public const int WorkerController_Delete = 50540;

    }
}
