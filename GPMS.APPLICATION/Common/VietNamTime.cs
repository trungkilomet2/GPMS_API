using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Common
{
    public static class VietnamTime
    {
        private static readonly TimeZoneInfo _timezone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        public static DateTime Now() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timezone);
    }
}
