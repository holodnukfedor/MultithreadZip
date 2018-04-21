using System;
using System.Collections;
using System.Threading;

namespace ZipVeeamTest
{
    public class ProcessingThreadBlocksQueue : SizeLimitedPriorityQueue<DataBlock>
    {
        private Queue _synchronizedQueue;

        private LimiterReadingThreadByOperMemory _limiterReadingThreadByOperMemory;

        protected override bool EndOfWaitingForAddition()
        {
            return _synchronizedQueue.Count > 0;
        }

        protected override void BeforeEnqueue()
        {
            while (IsMaxSizeExceeded() || _limiterReadingThreadByOperMemory.ProcessedBlocksCollectionSizeLimitExceed)
            {
                _limiterReadingThreadByOperMemory.CanReadByOperatMemoryLimitsEvent.Reset();
                _limiterReadingThreadByOperMemory.CanReadByOperatMemoryLimitsEvent.WaitOne();
            }
        }

        protected override void AddToCollection(DataBlock dataBlock)
        {
            _synchronizedQueue.Enqueue(dataBlock);
        }

        protected override DataBlock DequeueFromCollection()
        {
            return _synchronizedQueue.Dequeue() as DataBlock;
        }

        protected override bool NeedAwakeWaitingsForAdding()
        {
            return _synchronizedQueue.Count == 1;
        }

        protected override void AwakeAddingThread()
        {
            _limiterReadingThreadByOperMemory.CanReadByOperatMemoryLimitsEvent.Set();
        }

        public ProcessingThreadBlocksQueue(EndTaskEvent readEndEvent, int countLimit, LimiterReadingThreadByOperMemory limiterReadingThreadByOperMemory)
            : base(readEndEvent, countLimit, Queue.Synchronized(new Queue(countLimit)))
        {
            if (limiterReadingThreadByOperMemory == null)
                throw new ArgumentNullException("limiterReadingThreadByOperMemory");

            _limiterReadingThreadByOperMemory = limiterReadingThreadByOperMemory;
            _synchronizedQueue = _collection as Queue;
        }
    }
}
