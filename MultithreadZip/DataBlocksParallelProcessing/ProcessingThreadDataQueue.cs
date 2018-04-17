using System;
using System.Collections;
using System.Threading;

namespace ZipVeeamTest
{
    public class ProcessingThreadDataQueue
    {
        private const int _waitCount = 4000;

        private int _countLimit;

        private Object _locker = new Object();

        private EndTaskEvent _readEndEvent;

        private ManualResetEvent _blockReadOrReadEndedEvent;

        private Queue _synchronizedQueue;

        private LimiterReadingThreadByOperMemory _limiterReadingThreadByOperMemory;

        private void WaitOne()
        {
            int start = 0;

            while (Thread.VolatileRead(ref start) < _waitCount)
            {
                if (_synchronizedQueue.Count > 0 || _readEndEvent.IsEnded())
                    return;

                Thread.VolatileWrite(ref start, start + 1);
            }

            _blockReadOrReadEndedEvent.WaitOne();
        }

        public void AwakeTheWaitings()
        {
            _blockReadOrReadEndedEvent.Set();
        }

        public bool IsMaxSizeExceeded()
        {
            return _synchronizedQueue.Count > _countLimit;
        }

        public void Enqueue(DataBlock dataBlock)
        {
            while (IsMaxSizeExceeded() || _limiterReadingThreadByOperMemory.ProcessedBlocksCollectionSizeLimitExceed)
            {
                _limiterReadingThreadByOperMemory.CanReadByOperatMemoryLimitsEvent.Reset();
                _limiterReadingThreadByOperMemory.CanReadByOperatMemoryLimitsEvent.WaitOne();
            }
                    
            lock (_locker)
            {
                _synchronizedQueue.Enqueue(dataBlock);

                if (_synchronizedQueue.Count == 1)
                    _blockReadOrReadEndedEvent.Set();
            }
        }

        public DataBlock DequeueWait()
        {
            if (_synchronizedQueue.Count == 0 && !_readEndEvent.IsEnded())
                WaitOne();

            if (_synchronizedQueue.Count > 0)
            {
                DataBlock readBlock;

                lock (_locker)
                {
                    readBlock = _synchronizedQueue.Dequeue() as DataBlock;
                    _blockReadOrReadEndedEvent.Reset();

                    if (_synchronizedQueue.Count == 0 && !_readEndEvent.IsEnded())
                        _limiterReadingThreadByOperMemory.CanReadByOperatMemoryLimitsEvent.Set();
                        
                }

                return readBlock;
            }

            return null;
        }

        public int Count
        {
            get { return _synchronizedQueue.Count; }
        }

        public ProcessingThreadDataQueue(EndTaskEvent readEndEvent, int countLimit, LimiterReadingThreadByOperMemory limiterReadingThreadByOperMemory)
        {
            if (readEndEvent == null)
                throw new ArgumentException("readEndEvent");

            if (countLimit <= 0)
                throw new ArgumentException("Предел размера коллекции должен быть положительным");

            if (limiterReadingThreadByOperMemory == null)
                throw new ArgumentNullException("limiterReadingThreadByOperMemory");

            _limiterReadingThreadByOperMemory = limiterReadingThreadByOperMemory;
            _countLimit = countLimit;
            _readEndEvent = readEndEvent;
            _blockReadOrReadEndedEvent = new ManualResetEvent(false);
            _synchronizedQueue = Queue.Synchronized(new Queue(countLimit));
        }
    }
}
