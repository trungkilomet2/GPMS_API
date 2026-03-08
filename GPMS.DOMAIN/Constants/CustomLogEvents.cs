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

    }
}
