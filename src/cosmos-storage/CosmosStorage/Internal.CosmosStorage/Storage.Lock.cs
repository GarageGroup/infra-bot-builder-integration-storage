using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task<StorageLockStatus> LockAsync(string key, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<StorageLockStatus>(cancellationToken);
        }

        ThrowIfDisposed();
        return InnerLockAsync(key ?? string.Empty, cancellationToken);
    }

    private async Task<StorageLockStatus> InnerLockAsync(string key, CancellationToken cancellationToken)
    {
        var itemPath = ParseItemLockPath(key);
        var input = new StorageItemLockIn(itemPath)
        {
            ContainerTtlSeconds = GetTtlSeconds(CosmosStorageContainerType.LockStorage)
        };

        var result = await lazyCosmosApi.Value.LockItemAsync(input, cancellationToken).ConfigureAwait(false);
        return result.MapSuccess(MapSuccess).SuccessOrThrow(CreateException);

        static StorageLockStatus MapSuccess(StorageItemLockOut @out)
            =>
            @out switch
            {
                StorageItemLockOut.AlreadyLocked => StorageLockStatus.AlreadyLocked,
                _ => StorageLockStatus.Success
            };

        InvalidOperationException CreateException(StorageItemLockFailure failure)
            =>
            new($"Lock operation for key '{key}' has failed: {failure.FailureMessage}");
    }
}