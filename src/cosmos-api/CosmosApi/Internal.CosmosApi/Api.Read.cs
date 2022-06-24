using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class CosmosApi
{
    public ValueTask<Result<StorageItemReadOut, StorageItemReadFailure>> ReadItemAsync(
        StorageItemPath path, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<StorageItemReadOut, StorageItemReadFailure>>(cancellationToken);
        }

        return ValidatePath(path).MapFailure(MapFailure).ForwardValueAsync(InnerInvokeAsync);

        static StorageItemReadFailure MapFailure(Failure<Unit> failure)
            =>
            new(StorageItemReadFailureCode.InvalidPath, failure.FailureMessage);

        ValueTask<Result<StorageItemReadOut, StorageItemReadFailure>> InnerInvokeAsync(StorageItemPath itemPath)
            =>
            InnerReadItemAsync(itemPath, cancellationToken);
    }

    private async ValueTask<Result<StorageItemReadOut, StorageItemReadFailure>> InnerReadItemAsync(
        StorageItemPath path, CancellationToken cancellationToken)
    {
        var request = new StorageItemRequest(
            hashAlgorithm: lazyHmacSha256.Value,
            method: HttpMethod.Get,
            baseAddress: baseAddress,
            databaseId: databaseId,
            containerId: CreateContainerId(path),
            itemId: path.UserId);

        var result = await messageHandler.SendRequestAsync<StorageItemJsonRead>(request, true, cancellationToken).ConfigureAwait(false);
        return result.Map(MapSuccess, MapFailure);

        static StorageItemReadOut MapSuccess(StorageItemJsonRead itemJson)
            =>
            new(
                key: itemJson.Key,
                value: itemJson.Value?.ToValueRead());

        static StorageItemReadFailure MapFailure(StorageHttpFailure httpFailure)
            =>
            httpFailure.FailureCode switch
            {
                HttpStatusCode.NotFound => new(StorageItemReadFailureCode.NotFound, httpFailure.FailureMessage),
                _ => new(default, httpFailure.FailureMessage)
            };
    }
}