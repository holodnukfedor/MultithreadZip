using System;
using System.Threading;

namespace ZipVeeamTest
{
    public class LimiterReadingThreadByOperMemory
    {
        public volatile bool ProcessedBlocksCollectionSizeLimitExceed;

        public AutoResetEvent CanReadByOperatMemoryLimitsEvent { get; private set; }

        public LimiterReadingThreadByOperMemory()
        {
            CanReadByOperatMemoryLimitsEvent = new AutoResetEvent(false);
        }
    }
}
