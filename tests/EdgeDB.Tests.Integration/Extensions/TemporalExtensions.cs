using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration
{
    internal static class TemporalExtensions
    {
        public static TimeSpan RoundToMicroseconds(this TimeSpan t)
        {
            return TimeSpan.FromMicroseconds(Math.Round(t.TotalMicroseconds));
        }

        public static DateTime RoundToMicroseconds(this DateTime t)
        {
            // divide by 10 to cut off 100-nanosecond component
            var microseconds = Math.Round(t.Ticks / 10d);

            var a = t.AddMicroseconds(Math.Round(microseconds) != microseconds ? 1 : 0);

            var r = new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, a.Second, a.Millisecond, a.Microsecond);

            return r;
        }

        public static DateTimeOffset RoundToMicroseconds(this DateTimeOffset t)
        {
            // divide by 10 to cut off 100-nanosecond component
            return new DateTimeOffset(t.Ticks / 10, t.Offset);
        }

        public static TimeOnly RoundToMicroseconds(this TimeOnly t)
        {
            // divide by 10 to cut off 100-nanosecond component
            return new TimeOnly(t.Ticks / 10);
        }
    }
}
