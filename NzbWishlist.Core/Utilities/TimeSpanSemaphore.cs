using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Utilities
{
    // shamelessly taken from https://joelfillmore.wordpress.com/2011/04/01/throttling-web-api-calls/

    internal class TimeSpanSemaphore : IDisposable
    {
        private SemaphoreSlim _pool;
        private TimeSpan _resetSpan;
        private Queue<DateTime> _releaseTimes;

        private object _queueLock = new object();

        public TimeSpanSemaphore(int maxCount, TimeSpan resetSpan)
        {
            _pool = new SemaphoreSlim(maxCount, maxCount);
            _resetSpan = resetSpan;

            _releaseTimes = new Queue<DateTime>(
                collection: Enumerable.Range(0, maxCount).Select(n => DateTime.MinValue)
            );
        }

        public void Run(Action action, CancellationToken cancellationToken)
        {
            Wait(cancellationToken);

            try
            {
                action();
            }
            finally
            {
                Release();
            }
        }

        public async Task RunAsync(Func<Task> action, CancellationToken cancellationToken)
        {
            Wait(cancellationToken);

            try
            {
                await action().ConfigureAwait(false);
            }
            finally
            {
                Release();
            }
        }

        public async Task<TR> RunAsync<T, TR>(Func<T, CancellationToken, Task<TR>> action, T arg, CancellationToken cancellationToken)
        {
            Wait(cancellationToken);

            try
            {
                return await action(arg, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                Release();
            }
        }

        private void Wait(CancellationToken cancellationToken)
        {
            _pool.Wait(cancellationToken);

            DateTime oldestRelease;
            lock (_queueLock)
            {
                oldestRelease = _releaseTimes.Dequeue();
            }

            var now = DateTime.UtcNow;
            var windowReset = oldestRelease.Add(_resetSpan);
            if (windowReset > now)
            {
                int sleepMs = Math.Max((int)(windowReset.Subtract(now).Ticks / TimeSpan.TicksPerMillisecond), 1);
                Debug.WriteLine($"Waiting {sleepMs}ms for TimeSpanSemaphore limit to reset.");

                bool cancelled = cancellationToken.WaitHandle.WaitOne(sleepMs);
                if (cancelled)
                {
                    Release();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        private void Release()
        {
            lock (_queueLock)
            {
                _releaseTimes.Enqueue(DateTime.UtcNow);
            }
            _pool.Release();
        }

        public void Dispose() => _pool.Dispose();
    }
}
