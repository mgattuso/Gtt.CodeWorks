using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public static class ServiceClock
    {
        public static Func<DateTimeOffset> CurrentTime = () => DateTimeOffset.UtcNow;

        public static void ResetToUtc()
        {
            CurrentTime = () => DateTimeOffset.UtcNow;
        }
    }
}