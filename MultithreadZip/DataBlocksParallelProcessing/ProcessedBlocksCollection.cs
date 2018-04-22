using System;
using System.Collections;

namespace ZipVeeamTest
{
    public class ProcessedBlocksQueue : DataBlockPriorityQueueAbstract<byte []>
    {
        private Hashtable _processedBlocksTable;

        private int _awaitedKey;

        private int _lastAddedNumber;

        protected override bool EndOfWaitingForAddition()
        {
            return _processedBlocksTable.ContainsKey(_awaitedKey);
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

        public ProcessedBlocksQueue(EndTaskEvent processingEndEvent, int countLimit) : base(
            processingEndEvent,
            countLimit,
            Hashtable.Synchronized(new Hashtable(countLimit)),
            false
            )
        {
            _processedBlocksTable = _collection as Hashtable;
            _awaitedKey = 1;
        }
    }
}
