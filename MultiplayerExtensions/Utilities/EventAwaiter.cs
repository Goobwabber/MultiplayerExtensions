using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Utilities
{
    public class EventAwaiter : IDisposable
    {
        private TaskCompletionSource<bool> _taskCompletion;
        private CancellationTokenRegistration TokenRegistration;
        private bool disposedValue;

        public Task Task => _taskCompletion.Task;

        public EventAwaiter()
        {
            _taskCompletion = new TaskCompletionSource<bool>();
        }
        public EventAwaiter(CancellationToken cancellationToken)
            : this()
        {
            if (cancellationToken.CanBeCanceled)
            {
                TokenRegistration = cancellationToken.Register(Cancel);
            }
        }

        public void Cancel()
        {
            TriggerFinished(false);
            TokenRegistration.Dispose();
        }

        protected void TriggerFinished(bool success = true)
        {
            _taskCompletion.TrySetResult(success);
            TokenRegistration.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TokenRegistration.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class EventAwaiter<T1, T2> : EventAwaiter
    {
        public EventAwaiter()
            : base()
        { }
        public EventAwaiter(CancellationToken cancellationToken)
            : base(cancellationToken)
        { }

        public void OnEvent(T1 _, T2 __)
        {
            TriggerFinished(true);
        }

    }
}
