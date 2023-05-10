using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task WriteAsync(IDictionary<string, object?> changes, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        ThrowIfDisposed();

        if (changes is null || changes.Count is default(int))
        {
            return Task.CompletedTask;
        }

        return InnerWriteAsync(changes, cancellationToken);
    }

    private async Task InnerWriteAsync(IDictionary<string, object?> changes, CancellationToken cancellationToken)
    {
        foreach (var change in changes)
        {
            var value = change.Value;
            if (value is not null)
            {
                await InnerWriteItemAsync(change.Key, value, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await InnerDeleteItemAsync(change.Key, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task InnerWriteItemAsync(string key, object value, CancellationToken cancellationToken)
    {
        var itemPath = ParseItemPath(key);

        var input = new StorageItemWriteIn(
            path: itemPath,
            key: key,
            value: value,
            containerTtlSeconds: GetTtlSeconds(itemPath.ItemType));

        var result = await lazyCosmosApi.Value.WriteItemAsync(input, cancellationToken).ConfigureAwait(false);

        _ = result.Fold(InnerPipeSelf, ThrowFailure);
    }

    private static Unit ThrowFailure(StorageItemWriteFailure failure)
        =>
        throw new InvalidOperationException(failure.FailureMessage);
}