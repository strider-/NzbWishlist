using Microsoft.Azure.WebJobs.Extensions.Timers;
using System;

namespace NzbWishlist.Tests.Fixtures
{
    public class TimerScheduleStub : TimerSchedule
    {
        public override DateTime GetNextOccurrence(DateTime now)
        {
            throw new NotImplementedException();
        }
    }
}
