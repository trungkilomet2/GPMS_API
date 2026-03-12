using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Constants
{
    public class OrderStatus_Constants
    {
        public const string Pending = "Chờ Xét Duyệt";
        public const string Modification = "Yêu Cầu Chỉnh Sửa";
        public const string Approved = "Đã Chấp Nhận";
        public const string Rejected = "Đã Từ Chối";
        public const string Cancelled = "Đã Hủy";
    }
}