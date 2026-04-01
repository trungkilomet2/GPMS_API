using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Constants
{
    public class OrderStatus_Constants
    {

        public const int Pending_ID = 1;
        public const int Modification_ID = 2;
        public const int Approved_ID = 3;
        public const int Rejected_ID = 4;
        public const int Cancelled_ID = 5;
        public const int Producting_ID = 6;
        public const int Done_ID = 7;


        public const string Pending = "Chờ Xét Duyệt";
        public const string Modification = "Yêu Cầu Chỉnh Sửa";
        public const string Approved = "Đã Chấp Nhận";
        public const string Rejected = "Đã Từ Chối";
        public const string Cancelled = "Đã Hủy";
        public const string Producting = "Đang Sản Xuất";
        public const string Done = "Đã Hoàn Thành";


    }
}