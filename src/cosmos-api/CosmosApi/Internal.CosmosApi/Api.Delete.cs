using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosApi
{
    public ValueTask<Result<Unit, StorageItemDeleteFailure>> DeleteItemAsync(
        StorageItemPath? path, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<Unit, StorageItemDeleteFailure>>(cancellationToken);
        }

        ThrowIfDisposed();
        return ValidatePath(path).MapFailure(MapFailure).ForwardValueAsync(InnerInvokeAsync);

        static StorageItemDeleteFailure MapFailure(Failure<Unit> failure)
            =>
            new(StorageItemDeleteFailureCode.InvalidPath, failure.FailureMessage);

        ValueTask<Result<Unit, StorageItemDeleteFailure>> InnerInvokeAsync(StorageItemPath @in)
            =>
            InnerDeleteItemAsync(@in, cancellationToken);
    }

    private async ValueTask<Result<Unit, StorageItemDeleteFailure>> InnerDeleteItemAsync(
        StorageItemPath path, CancellationToken cancellationToken)
    {
        var request = new StorageItemRequest(
            hashAlgorithm: lazyHmacSha256.Value,
            method: HttpMethod.Delete,
            baseAddress: baseAddress,
            databaseId: databaseId,
            containerId: CreateContainerId(path),
            itemId: path.UserId);

        var result = await messageHandler.SendRequestAsync<Unit>(request, false, cancellationToken).ConfigureAwait(false);
        return result.MapFailure(MapFailure);

        static StorageItemDeleteFailure MapFailure(StorageHttpFailure httpFailure)
            =>
            httpFailure.FailureCode switch
            {
                HttpStatusCode.NotFound => new(StorageItemDeleteFailureCode.NotFound, httpFailure.FailureMessage),
                _ => new(default, httpFailure.FailureMessage)
            };
    }
}