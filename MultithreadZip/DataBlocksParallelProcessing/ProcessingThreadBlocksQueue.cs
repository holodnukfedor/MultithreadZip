using System;
using System.Collections;
using System.Threading;

namespace ZipVeeamTest
{
    public class ProcessingThreadBlocksQueue : DataBlockPriorityQueueAbstract<DataBlock>
    {
        private Queue _synchronizedQueue;

        protected override bool EndOfWaitingForAddition()
        {
            return _synchronizedQueue.Count > 0;
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

        public ProcessingThreadBlocksQueue(EndTaskEvent readEndEvent, int countLimit)
            : base(
                readEndEvent,
                countLimit,
                Queue.Synchronized(new Queue(countLimit)), 
                true
            )
        {
            _synchronizedQueue = _collection as Queue;
        }
    }
}
