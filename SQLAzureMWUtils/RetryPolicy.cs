using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    public class RetryPolicy
    {
        public int RetryCount { get; set; }
        public TimeSpan MinimunDelay { get; set; }
        public TimeSpan MaximunDelay { get; set; }
        public TimeSpan RetryIncrementalDelay { get; set; }

        private RetryPolicy()
        {
        }

        public RetryPolicy(int retryCount, TimeSpan minimunDelay, TimeSpan maximunDelay, TimeSpan incrementalDelay)
        {
            RetryCount = retryCount;
            MinimunDelay = minimunDelay;
            MaximunDelay = maximunDelay;
            RetryIncrementalDelay = incrementalDelay;
        }

        public bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay)
        {
            if (retryCount < RetryCount)
            {
                var random = new Random();

                var delta = (int)((Math.Pow(2.0, retryCount) - 1.0) * random.Next((int)(RetryIncrementalDelay.TotalMilliseconds * 0.8), (int)(RetryIncrementalDelay.TotalMilliseconds * 1.2)));
                var interval = (int) Math.Min(checked(MinimunDelay.TotalMilliseconds + delta), MaximunDelay.TotalMilliseconds);

                delay = TimeSpan.FromMilliseconds(interval);

                return true;
            }

            delay = TimeSpan.Zero;
            return false;
        }
    }
}