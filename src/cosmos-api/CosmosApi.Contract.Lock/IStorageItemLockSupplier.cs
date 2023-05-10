using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

public interface IStorageItemLockSupplier : IDisposable
{
    ValueTask<Result<StorageItemLockOut, StorageItemLockFailure>> LockItemAsync(
        StorageItemLockIn input, CancellationToken cancellationToken = default);

    ValueTask<Result<Unit, StorageItemLockFailure>> UnlockItemAsync(
        StorageItemLockPath input, CancellationToken cancellationToken = default);
}