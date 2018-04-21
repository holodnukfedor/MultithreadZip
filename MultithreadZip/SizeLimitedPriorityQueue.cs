using System;
using System.Collections;
using System.Threading;

namespace ZipVeeamTest
{
    public abstract class SizeLimitedPriorityQueue<TGetType> where TGetType : class
    {
        private const int _waitCount = 4000;

        private readonly int _countLimit;

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

        protected abstract void BeforeEnqueue();

        protected abstract void AddToCollection(DataBlock dataBlock);

        protected abstract TGetType DequeueFromCollection();

        protected abstract bool NeedAwakeWaitingsForAdding();

        protected virtual void AwakeAddingThread()
        {

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
            BeforeEnqueue();

            lock (_collectionChangedLocker)
            {
                AddToCollection(dataBlock);

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
                    result = DequeueFromCollection();
                    _itemAddedEvent.Reset();
                }

                return result;
            }

            return null;
        }

        public SizeLimitedPriorityQueue(EndTaskEvent addToCollectionEndEvent, int countLimit, ICollection collection)
        {
            if (addToCollectionEndEvent == null)
                throw new ArgumentException("readEndEvent");

            if (countLimit <= 0)
                throw new ArgumentException("Предел размера коллекции должен быть положительным");

            if (collection == null)
                throw new ArgumentException("Коллекция не должна быть равна null");

            _countLimit = countLimit;
            _addToCollectionEndEvent = addToCollectionEndEvent;
            _collection = collection;
        }
    }
}
