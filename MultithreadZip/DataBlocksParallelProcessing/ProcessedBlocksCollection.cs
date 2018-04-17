using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZipVeeamTest
{
    public class ProcessedBlocksCollection
    {
        private const int _waitCount = 4000;

        private int _countLimit;

        private Object _locker = new Object();

        private EndTaskEvent _processingEndEvent;

        private ManualResetEvent _blockProcessedOrProcessEndEvent;

        private Hashtable _processedBlocksTable;

        private LimiterReadingThreadByOperMemory _limiterReadingThreadByOperMemory;

        private int _awaitedKey;

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

        private void AwakeReadingThread()
        {
            if (_processedBlocksTable.Count == 0)
            {
                _limiterReadingThreadByOperMemory.ProcessedBlocksCollectionSizeLimitExceed = false;
                _limiterReadingThreadByOperMemory.CanReadByOperatMemoryLimitsEvent.Set();
            }
        }

        public void AwakeTheWaitings()
        {
            _blockProcessedOrProcessEndEvent.Set();
        }

        public bool IsMaxSizeExceeded()
        {
            return _processedBlocksTable.Count > _countLimit;
        }

        public void Add(int number, byte [] buffer)
        {
            if (IsMaxSizeExceeded())
                _limiterReadingThreadByOperMemory.ProcessedBlocksCollectionSizeLimitExceed = true;

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
            {
                AwakeReadingThread();
                WaitOne();
            }
                
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

        public ProcessedBlocksCollection(EndTaskEvent processingEndEvent, int countLimit, LimiterReadingThreadByOperMemory limiterReadingThreadByOperMemory)
        {
            if (processingEndEvent == null)
                throw new ArgumentNullException("processingEndEvent");

            if (countLimit <= 0)
                throw new ArgumentException("лимит количества элементов в коллекции должен быть равен ");

            if (limiterReadingThreadByOperMemory == null)
                throw new ArgumentNullException("limiterReadingThreadByOperMemory");

            _limiterReadingThreadByOperMemory = limiterReadingThreadByOperMemory;
            _countLimit = countLimit;
            _processingEndEvent = processingEndEvent;
            _processedBlocksTable = Hashtable.Synchronized(new Hashtable(countLimit));
            _blockProcessedOrProcessEndEvent = new ManualResetEvent(false);
            _awaitedKey = 1;
        }
    }
}
