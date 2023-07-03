using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class SemaphoreExtensions
{
    internal static async Task InvokeAsync<TIn>(
        this SemaphoreSlim semaphore, Func<TIn, CancellationToken, Task> func, TIn input, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await func.Invoke(input, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }
}