using System;

namespace ProjectQ.Models
{
    public static class ApiCounter
    {
        private const int MaxCallsPerMin = 60;
        public static int CallCount { get; set; }
        public static DateTime LastReset { get; set; }

        static ApiCounter()
        {
            CallCount = 0;
            LastReset = DateTime.Now;
        }

        /// <summary>
        /// Returns true if call can be made and increments counter. False is call quota has been used up.
        /// </summary>
        /// <returns></returns>
        public static bool TryCall()
        {
            if (LastReset.AddMinutes(1) < DateTime.Now)
            {
                LastReset = DateTime.Now;
                CallCount = 1;
                return true;
            }

            if (CallCount >= MaxCallsPerMin)
                return false;

            CallCount++;
            return true;

        }
    }
}