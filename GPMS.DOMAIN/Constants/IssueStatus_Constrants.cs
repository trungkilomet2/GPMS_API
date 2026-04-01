using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Constants
{
    public class IssueStatus_Constrants
    {

        public const int ToDo_ID = 1;
        public const int Processing_ID = 2;
        public const int Fixed_ID = 3;
        public const int Error_ID = 4;

        public const string ToDo = "Chờ Xử Lý";
        public const string Processing = "Đang Xử Lý";
        public const string Fixed = "Đã Khắc Phục";
        public const string Error = "Không Thể Sửa";
        
        // Workflow 
        /*
            To Do -> Proccessing -> Fixed
            To Do -> Error
         */



    }
}
