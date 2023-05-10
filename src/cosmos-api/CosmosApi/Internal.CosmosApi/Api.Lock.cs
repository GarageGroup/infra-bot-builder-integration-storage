using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosApi
{
    public ValueTask<Result<StorageItemLockOut, StorageItemLockFailure>> LockItemAsync(
        StorageItemLockIn input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<StorageItemLockOut, StorageItemLockFailure>>(cancellationToken);
        }

        ThrowIfDisposed();
        return InnerLockItemAsync(input, cancellationToken);
    }

    public ValueTask<Result<Unit, StorageItemLockFailure>> UnlockItemAsync(
        StorageItemLockPath input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<Unit, StorageItemLockFailure>>(cancellationToken);
        }

        ThrowIfDisposed();
        return InnerUnlockItemAsync(input, cancellationToken);
    }

    private async ValueTask<Result<StorageItemLockOut, StorageItemLockFailure>> InnerLockItemAsync(
        StorageItemLockIn input, CancellationToken cancellationToken)
    {
        var requestCreate = new StorageItemRequest(
            hashAlgorithm: lazyHmacSha256.Value,
            method: HttpMethod.Post,
            baseAddress: baseAddress,
            databaseId: databaseId,
            containerId: CreateContainerId(input.Path),
            itemId: input.Path.ItemId,
            content: new StorageItemJsonWrite(
                id: input.Path.ItemId,
                key: input.Path.ItemId,
                value: null));

        var resultCreateFirst = await SendStorageItemRequestAsync(requestCreate).ConfigureAwait(false);
        if (resultCreateFirst.IsSuccess)
        {
            return StorageItemLockOut.Success;
        }

        var failureCreateFirst = resultCreateFirst.FailureOrThrow();
        if (failureCreateFirst.FailureCode is HttpStatusCode.Conflict)
        {
            return StorageItemLockOut.AlreadyLocked;
        }

        if (failureCreateFirst.FailureCode is not HttpStatusCode.NotFound)
        {
            return ToUnknownFailure(failureCreateFirst);
        }

        var containerResult = await EnsureContainerAsync().ConfigureAwait(false);
        if (containerResult.IsFailure)
        {
            var containerFailure = containerResult.FailureOrThrow();
            if (containerFailure.FailureCode is not HttpStatusCode.Conflict)
            {
                return ToUnknownFailure(containerFailure);
            }
        }

        var resultCreateSecond = await SendStorageItemRequestAsync(requestCreate).ConfigureAwait(false);
        if (resultCreateSecond.IsSuccess)
        {
            return StorageItemLockOut.Success;
        }

        var failureCreateSecond = resultCreateSecond.FailureOrThrow();
        if (failureCreateSecond.FailureCode is HttpStatusCode.Conflict)
        {
            return StorageItemLockOut.AlreadyLocked;
        }

        return ToUnknownFailure(failureCreateSecond);

        ValueTask<Result<Unit, StorageHttpFailure>> EnsureContainerAsync()
        {
            if (IsContainerExisted(failureCreateFirst))
            {
                return new(default(Unit));
            }

            var containerRequest = new StorageContainerRequest(
                hashAlgorithm: lazyHmacSha256.Value,
                baseAddress: baseAddress,
                databaseId: databaseId,
                content: new(
                    id: requestCreate.ContainerId,
                    defaultTtlSeconds: input.ContainerTtlSeconds,
                    partitionKey: partitionKey));

            return messageHandler.SendRequestAsync(containerRequest, cancellationToken);
        }

        ValueTask<Result<Unit, StorageHttpFailure>> SendStorageItemRequestAsync(StorageItemRequest request)
            =>
            messageHandler.SendRequestAsync<Unit>(request, false, cancellationToken);

        static StorageItemLockFailure ToUnknownFailure(StorageHttpFailure httpFailure)
            =>
            new(httpFailure.FailureMessage);
    }

    private async ValueTask<Result<Unit, StorageItemLockFailure>> InnerUnlockItemAsync(
        StorageItemLockPath input, CancellationToken cancellationToken)
    {
        var request = new StorageItemRequest(
            hashAlgorithm: lazyHmacSha256.Value,
            method: HttpMethod.Delete,
            baseAddress: baseAddress,
            databaseId: databaseId,
            containerId: CreateContainerId(input),
            itemId: input.ItemId);

        var result = await messageHandler.SendRequestAsync<Unit>(request, false, cancellationToken).ConfigureAwait(false);
        return result.Recover(MapFailure);

        static Result<Unit, StorageItemLockFailure> MapFailure(StorageHttpFailure httpFailure)
            =>
            httpFailure.FailureCode switch
            {
                HttpStatusCode.NotFound => default(Unit),
                _ => new StorageItemLockFailure(httpFailure.FailureMessage)
            };
    }
}