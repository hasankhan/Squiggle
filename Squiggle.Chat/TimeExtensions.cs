using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    public static class TimeExtensions
    {
        public static TimeSpan Seconds(this int number)
        {
            return TimeSpan.FromSeconds(number);
        }

        public static TimeSpan Minutes(this int number)
        {
            return TimeSpan.FromMinutes(number);
        }
    }
}
