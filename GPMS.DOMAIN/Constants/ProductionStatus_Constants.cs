using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Constants
{
    public class ProductionStatus_Constants
    {
        //(N'Chờ Xét Duyệt'),
        //(N'Từ Chối'),
        //(N'Cần Cập Nhật'),
        //(N'Chấp Nhận'),
        //(N'Chờ Xét Duyệt Kế Hoạch'),
        //(N'Cần Chỉnh Sửa Kế Hoạch'),
        //(N'Đang Sản Xuất'),
        //(N'Hoàn Thành');
        // ID
        public const int Pending_ID = 1; //1
        public const int Reject_ID = 2; //2 
        public const int Approval_ID = 3; //3
        public const int PendingPlan_ID = 4; //4
        public const int NeedUpdatePlan_ID = 5; //5
        public const int Producting_ID = 6; //6 
        public const int Done_ID = 7; //7

        // NAME
        public const string Pending = "Chờ xét duyệt"; //1
        public const string Reject = "Từ Chối"; //2 
        public const string Approval = "Chấp Nhận"; //3
        public const string PendingPlan = "Chờ Xét Duyệt Kế Hoạch"; //4
        public const string NeedUpdatePlan = "Cần Chỉnh Sửa Kế Hoạch"; //5
        public const string Producting = "Đang Sản Xuất"; //6 
        public const string Done = "Hoàn Thành"; //7


    }
}
