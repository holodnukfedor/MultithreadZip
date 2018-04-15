using System;
using System.Threading;
using System.Collections;

namespace ZipVeeamTest
{
    public class BlocksPreparedToWrite
    {
        private AutoResetEvent _autoResetEvent;

        public Hashtable ProcessedBlocksTable { get; private set; }

        public volatile int AwaitedKey;

        public void Set()
        {
            _autoResetEvent.Set();
        }

        public void Reset()
        {
            _autoResetEvent.Reset();
        }

        public void WaitOne(int awaitedKey, int countOfTicks = 4000)
        {
            int start = 0;
            while (Thread.VolatileRead(ref start) < countOfTicks)
            {
                if (ProcessedBlocksTable.ContainsKey(awaitedKey))
                    return;

                Thread.VolatileWrite(ref start, start + 1);
            }

            _autoResetEvent.WaitOne();
        }

        public BlocksPreparedToWrite(Hashtable hashTable)
        {
            if (hashTable == null)
                throw new ArgumentNullException("hashTable");

            if (!hashTable.IsSynchronized)
                throw new ArgumentException("хеш таблица должна быть синхронизированна");

            ProcessedBlocksTable = hashTable;
            _autoResetEvent = new AutoResetEvent(false);
            AwaitedKey = 1;
        }
    }
}
