using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Constants
{
    public class OrderSizeStatus_Constants
    {
        public const int Pending_Id = 1;
        public const int InProgress_Id = 2;
        public const int Completed_Id = 3;

        public const string Pending = "Chưa Sản Xuất";
        public const string InProgress = "Đang Sản Xuất";
        public const string Completed = "Hoàn Thành";
    }
}
