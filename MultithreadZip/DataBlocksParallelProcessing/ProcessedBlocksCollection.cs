using System;
using System.Collections;

namespace ZipVeeamTest
{
    public class ProcessedBlocksQueue : SizeLimitedPriorityQueue<byte []>
    {
        private Hashtable _processedBlocksTable;

        private LimiterReadingThreadByOperMemory _limiterReadingThreadByOperMemory;

        private int _awaitedKey;

        private int _lastAddedNumber;

        protected override bool EndOfWaitingForAddition()
        {
            return _processedBlocksTable.ContainsKey(_awaitedKey);
        }

        protected override void AwakeAddingThread()
        {
            if (_processedBlocksTable.Count == 0)
            {
                _limiterReadingThreadByOperMemory.ProcessedBlocksCollectionSizeLimitExceed = false;
                _limiterReadingThreadByOperMemory.CanReadByOperatMemoryLimitsEvent.Set();
            }
        }

        protected override void BeforeEnqueue()
        {
            _limiterReadingThreadByOperMemory.ProcessedBlocksCollectionSizeLimitExceed = true;
        }

        protected override void AddToCollection(DataBlock dataBlock)
        {
            _processedBlocksTable.Add(dataBlock.Number, dataBlock.Block);
            _lastAddedNumber = dataBlock.Number;
        }

        protected override bool NeedAwakeWaitingsForAdding()
        {
            return _lastAddedNumber == _awaitedKey;
        }

        protected override byte [] DequeueFromCollection()
        {
            Console.WriteLine(_awaitedKey);
            var buffer = _processedBlocksTable[_awaitedKey] as byte[];
            _processedBlocksTable.Remove(_awaitedKey);
            ++_awaitedKey;

            return buffer;
        }

        public ProcessedBlocksQueue(EndTaskEvent processingEndEvent, int countLimit, LimiterReadingThreadByOperMemory limiterReadingThreadByOperMemory) : base(processingEndEvent, countLimit, Hashtable.Synchronized(new Hashtable(countLimit)))
        {
            if (limiterReadingThreadByOperMemory == null)
                throw new ArgumentNullException("limiterReadingThreadByOperMemory");

            _limiterReadingThreadByOperMemory = limiterReadingThreadByOperMemory;
            _processedBlocksTable = _collection as Hashtable;
            _awaitedKey = 1;
        }
    }
}
