using System;

namespace Gtt.Simple.CodeWorks
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
