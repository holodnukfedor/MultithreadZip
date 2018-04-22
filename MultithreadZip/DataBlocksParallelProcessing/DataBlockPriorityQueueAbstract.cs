using System;
using System.Collections;
using System.Threading;

namespace ZipVeeamTest
{
    public abstract class DataBlockPriorityQueueAbstract<TGetType> where TGetType : class
    {
        private static object _limitReadByCollectionSizeLocker = new object();

        private static bool _processedBlockSizeLimitExceed = false;

        private const int _waitCount = 4000;

        private readonly int _countLimit;

        private readonly bool _isProcessingDataBlocksQueue;

        private Object _collectionChangedLocker = new Object();

        private ManualResetEvent _itemAddedEvent = new ManualResetEvent(false);

        private EndTaskEvent _addToCollectionEndEvent;

        protected ICollection _collection;

        private void WaitAdding()
        {
            int start = 0;

            while (Thread.VolatileRead(ref start) < _waitCount)
            {
                if (EndOfWaitingForAddition() || _addToCollectionEndEvent.IsEnded())
                    return;

                Thread.VolatileWrite(ref start, start + 1);
            }

            _itemAddedEvent.WaitOne();
        }

        protected bool IsMaxSizeExceeded()
        {
            return _collection.Count > _countLimit;
        }

        protected abstract bool EndOfWaitingForAddition();

        protected void CheckSizeLimitExceed()
        {
            lock (_limitReadByCollectionSizeLocker)
            {
                if (_isProcessingDataBlocksQueue)
                {
                    while (IsMaxSizeExceeded() || _processedBlockSizeLimitExceed)
                    {
                        Monitor.Wait(_limitReadByCollectionSizeLocker);
                    }
                }
                else
                {
                    if (IsMaxSizeExceeded())
                        _processedBlockSizeLimitExceed = true;
                }
            }
        }

        protected abstract void AddToCollection(DataBlock dataBlock);

        protected abstract TGetType DequeueFromCollection();

        protected abstract bool NeedAwakeWaitingsForAdding();

        protected void AwakeAddingThread()
        {
            lock (_limitReadByCollectionSizeLocker)
            {
                if (!_isProcessingDataBlocksQueue &&
                    _collection.Count == 0 &&
                    _processedBlockSizeLimitExceed)
                {
                    _processedBlockSizeLimitExceed = false;
                }

                Monitor.Pulse(_limitReadByCollectionSizeLocker);
            }
        }

        public int Count
        {
            get { return _collection.Count; }
        }

        public void AwakeWaitingsForAddition()
        {
            _itemAddedEvent.Set();
        }

        public void Enqueue(DataBlock dataBlock)
        {
            CheckSizeLimitExceed();

            lock (_collectionChangedLocker)
            {
                lock (_limitReadByCollectionSizeLocker)
                {
                    AddToCollection(dataBlock);
                }

                if (NeedAwakeWaitingsForAdding())
                    _itemAddedEvent.Set();
            }
        }

        public TGetType DequeueWait()
        {
            if (!EndOfWaitingForAddition() && !_addToCollectionEndEvent.IsEnded())
            {
                AwakeAddingThread();
                WaitAdding();
            }
                
            if (EndOfWaitingForAddition())
            {
                TGetType result;

                lock (_collectionChangedLocker)
                {
                    lock (_limitReadByCollectionSizeLocker)
                    {
                        result = DequeueFromCollection();
                    }

                    _itemAddedEvent.Reset();
                }

                return result;
            }

            return null;
        }

        public DataBlockPriorityQueueAbstract(
            EndTaskEvent addToCollectionEndEvent,
            int countLimit, 
            ICollection collection,
            bool isProcessingDataBlocksQueue
            )
        {
            if (addToCollectionEndEvent == null)
                throw new ArgumentException("readEndEvent");

            if (countLimit <= 0)
                throw new ArgumentException("Предел размера коллекции должен быть положительным");

            if (collection == null)
                throw new ArgumentException("Коллекция не должна быть равна null");

            _isProcessingDataBlocksQueue = isProcessingDataBlocksQueue;
            _countLimit = countLimit;
            _addToCollectionEndEvent = addToCollectionEndEvent;
            _collection = collection;
        }
    }
}
