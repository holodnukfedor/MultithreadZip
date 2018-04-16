using System;
using System.Threading;
using System.Collections;

namespace ZipVeeamTest
{
    public class ProcessedBlocksCollection
    {
        private const int _waitCount = 4000;

        private Object _locker = new Object();

        private EndTaskEvent _processingEndEvent;

        private ManualResetEvent _blockProcessedOrProcessEndEvent;

        private Hashtable _processedBlocksTable;

        private int _awaitedKey;

        public void AwakeTheWaitings()
        {
            _blockProcessedOrProcessEndEvent.Set();
        }

        private void WaitOne()
        {
            int start = 0;
            while (Thread.VolatileRead(ref start) < _waitCount)
            {
                if (_processedBlocksTable.ContainsKey(_awaitedKey) || _processingEndEvent.IsEnded())
                    return;

                Thread.VolatileWrite(ref start, start + 1);
            }

            _blockProcessedOrProcessEndEvent.WaitOne();
        }

        public void Add(int number, byte [] buffer)
        {
            lock(_locker)
            {
                _processedBlocksTable.Add(number, buffer);
                if (number == _awaitedKey)
                    _blockProcessedOrProcessEndEvent.Set();
            }
        }

        public byte [] GetWaitNextProcessedBlock()
        {
            if (!_processedBlocksTable.ContainsKey(_awaitedKey) && !_processingEndEvent.IsEnded())
                WaitOne();

            if (_processedBlocksTable.ContainsKey(_awaitedKey))
            {
                byte[] buffer;

                lock (_locker)
                {
                    Console.WriteLine(_awaitedKey);
                    buffer = _processedBlocksTable[_awaitedKey] as byte[];
                    _blockProcessedOrProcessEndEvent.Reset();
                    _processedBlocksTable.Remove(_awaitedKey);
                    ++_awaitedKey;
                }

                return buffer;
            }

            return null;
        }

        public ProcessedBlocksCollection(EndTaskEvent processingEndEvent)
        {
            if (processingEndEvent == null)
                throw new ArgumentNullException("processingEndEvent");

            _processingEndEvent = processingEndEvent;
            _processedBlocksTable = Hashtable.Synchronized(new Hashtable());
            _blockProcessedOrProcessEndEvent = new ManualResetEvent(false);
            _awaitedKey = 1;
        }
    }
}
