using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

public interface IStorageItemDeleteSupplier : IDisposable
{
    ValueTask<Result<Unit, StorageItemDeleteFailure>> DeleteItemAsync(
        StorageItemPath path, CancellationToken cancellationToken = default);
}