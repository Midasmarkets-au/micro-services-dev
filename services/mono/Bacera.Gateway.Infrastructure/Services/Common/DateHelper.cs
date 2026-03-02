using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bacera.Gateway.Services.Common
{
    public static class DateHelper
    {
        /// <summary>
        /// Convert MT5 time to MM time by subtracting hours gap
        /// </summary>
        public static (DateTime? FromMinus2H, DateTime? ToMinus2H) MinusMT5GMTHours(DateTime? from, DateTime? to, int hoursGap = 2)
        {
            return (
                from?.AddHours(-hoursGap),
                to?.AddHours(-hoursGap)
            );
        }

        /// <summary>
        /// Convert MT5 time to MM time by subtracting hours gap
        /// </summary>
        public static DateTime? MinusMT5GMTHours(DateTime? dateTime, int hoursGap = 2)
        {
            return dateTime?.AddHours(-hoursGap);
        }
    }
}
