using System;
using System.Threading;

namespace RabbitMqBinding
{
    /// <summary>
    /// Generic async class to handle signaling completion of async operation.
    /// </summary>
    /// <seealso cref="System.IAsyncResult" />
    internal class GenericAsyncResult : IAsyncResult
    {
        private readonly ManualResetEvent _WaitHandle;

        public GenericAsyncResult(bool setComplete, object asyncState, TimeSpan timeout, AsyncCallback callback, bool completedSynchronously)
        {
            Callback = callback;

            IsCompleted = setComplete;

            CompletedSynchronously = completedSynchronously;

            //if async op is complete then we'll set the wait handle to signaled right away. 
            _WaitHandle = new ManualResetEvent(setComplete); 

            AsyncState = asyncState;

            if (timeout < TimeSpan.MaxValue)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new Timer(obj =>
                    {
                        IsTimedOut = true;
                         
                        Complete();

                    },
                    null, 
                    (long)timeout.TotalMilliseconds, 
                    TimeSpan.MaxValue.Milliseconds);
            }
        }

        public AsyncCallback Callback { get; set; }

        public TimeSpan Timeout { get; set; }

        public object AsyncState { get; set; }

        public WaitHandle AsyncWaitHandle
        {
            get { return _WaitHandle; }
        }

        public bool CompletedSynchronously { get; }

        public bool IsCompleted { get; private set; }

        public void Complete()
        {
            _WaitHandle.Set();

            IsCompleted = true;

        }

        public bool IsTimedOut { get; set; }
        public RabbitMqMessage RabbitMessage { get; set; }
    }
}