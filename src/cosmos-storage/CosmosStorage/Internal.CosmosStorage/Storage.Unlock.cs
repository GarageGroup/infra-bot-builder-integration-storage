using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task UnlockAsync(string key, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        ThrowIfDisposed();
        return InnerUnlockAsync(key ?? string.Empty, cancellationToken);
    }

    private async Task InnerUnlockAsync(string key, CancellationToken cancellationToken)
    {
        var itemPath = ParseItemLockPath(key);

        var result = await lazyCosmosApi.Value.UnlockItemAsync(itemPath, cancellationToken).ConfigureAwait(false);
        _ = result.SuccessOrThrow(CreateException);

        InvalidOperationException CreateException(StorageItemLockFailure failure)
            =>
            new($"Unlock operation for key '{key}' has failed: {failure.FailureMessage}");
    }
}