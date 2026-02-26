using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Helpers
{
    public class DateTimeEva
    {
        public static DateTime ToEgyptTime(DateTime utcDate)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDate, egyptTimeZone);
        }
    }
}
