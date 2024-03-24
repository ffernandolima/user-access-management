using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace UserAccessManagement.Infrastructure.Threading
{
    public static class AsyncLocker
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

        public static async Task<IDisposable> LockAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }

            var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();

            var releaser = new AsyncLockerReleaser(semaphore);

            return releaser;
        }

        private sealed class AsyncLockerReleaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public AsyncLockerReleaser(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
            }

            public void Dispose()
            {
                _semaphore.Release();
            }
        }
    }
}
