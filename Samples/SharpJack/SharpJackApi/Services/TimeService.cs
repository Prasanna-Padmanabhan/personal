using System;

namespace SharpJackApi.Services
{
    /// <summary>
    /// Simulates the current time for predictable results in tests.
    /// </summary>
    public class TimeService
    {
        /// <summary>
        /// The simulated time.
        /// </summary>
        private DateTime? currentTime;

        /// <summary>
        /// If a value is set, return that; otherwise return current time.
        /// </summary>
        public DateTime CurrentTime
        {
            get
            {
                return currentTime ?? DateTime.UtcNow;
            }
            set
            {
                currentTime = value;
            }
        }
    }
}