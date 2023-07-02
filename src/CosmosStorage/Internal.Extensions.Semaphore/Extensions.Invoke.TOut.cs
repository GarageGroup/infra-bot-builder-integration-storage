using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class SemaphoreExtensions
{
    internal static async Task<TOut> InvokeAsync<TIn, TOut>(
        this SemaphoreSlim semaphore, Func<TIn, CancellationToken, Task<TOut>> func, TIn input, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await func.Invoke(input, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }
}