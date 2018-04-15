using System;
using System.Collections;
using System.Threading;

namespace ZipVeeamTest
{
    public class ProcessingThreadData
    {
        private AutoResetEvent _autoResetEvent;

        public Queue SynchronizedQueue { get; private set; }

        public void Set()
        {
            _autoResetEvent.Set();
        }

        public void Reset()
        {
            _autoResetEvent.Reset();
        }

        public void WaitOne(int countOfTicks = 4000)
        {
            int start = 0;
            while (Thread.VolatileRead(ref start) < countOfTicks)
            {
                if (SynchronizedQueue.Count > 0)
                    return;

                Thread.VolatileWrite(ref start, start + 1);
            }

            _autoResetEvent.WaitOne();
        }

        public ProcessingThreadData()
        {
            _autoResetEvent = new AutoResetEvent(false);
            SynchronizedQueue = Queue.Synchronized(new Queue());
        }
    }
}
