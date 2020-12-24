using System;
using System.Threading;

namespace SharpJackApi.Services
{
    public class TimeService
    {
        private DateTime? currentTime;

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