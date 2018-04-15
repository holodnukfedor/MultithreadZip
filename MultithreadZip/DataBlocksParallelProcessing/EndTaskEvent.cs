using System;
using System.Threading;

namespace ZipVeeamTest
{
    public class EndTaskEvent
    {
        private volatile bool _isTaskEnded;

        private AutoResetEvent _endEvent;

        public void SetEndTask()
        {
            _isTaskEnded = true;
            _endEvent.Set();
        }

        public bool IsEnded()
        {
            return _isTaskEnded;
        }

        public void WaitOne()
        {
            _endEvent.WaitOne();
        }

        public EndTaskEvent()
        {
            _endEvent = new AutoResetEvent(false);
        }
    }
}
