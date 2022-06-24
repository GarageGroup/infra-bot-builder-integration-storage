using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class CosmosApi
{
    public ValueTask<Result<Unit, StorageItemWriteFailure>> WriteItemAsync(
        StorageItemWriteIn? input, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<Unit, StorageItemWriteFailure>>(cancellationToken);
        }

        return ValidatePath(input?.Path).MapFailure(MapPathFailure).Forward(InnerValidateKey).ForwardValueAsync(InnerInvokeAsync);

        static StorageItemWriteFailure MapPathFailure(Failure<Unit> failure)
            =>
            new(StorageItemWriteFailureCode.InvalidPath, failure.FailureMessage);

        Result<StorageItemWriteIn, StorageItemWriteFailure> InnerValidateKey(StorageItemPath _)
            =>
            ValidateKey(input);

        ValueTask<Result<Unit, StorageItemWriteFailure>> InnerInvokeAsync(StorageItemWriteIn @in)
            =>
            InnerWriteItemAsync(@in, cancellationToken);
    }

    private async ValueTask<Result<Unit, StorageItemWriteFailure>> InnerWriteItemAsync(
        StorageItemWriteIn input, CancellationToken cancellationToken)
    {
        var requestUpdate = new StorageItemRequest(
            hashAlgorithm: lazyHmacSha256.Value,
            method: HttpMethod.Put,
            baseAddress: baseAddress,
            databaseId: databaseId,
            containerId: CreateContainerId(input.Path),
            itemId: input.Path.UserId,
            content: new StorageItemJsonWrite(
                id: input.Path.UserId,
                key: input.Key,
                value: input.Value.ToValueWrite()));

        var resultUpdate = await SendStorageItemRequestAsync(requestUpdate).ConfigureAwait(false);
        if (resultUpdate.IsSuccess)
        {
            return default(Unit);
        }

        var failureUpdate = resultUpdate.FailureOrThrow();
        if (failureUpdate.FailureCode is not HttpStatusCode.NotFound)
        {
            return ToUnknownFailure(failureUpdate);
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

        var requestCreate = requestUpdate with
        {
            Method = HttpMethod.Post
        };

        var resultCreate = await SendStorageItemRequestAsync(requestCreate).ConfigureAwait(false);
        if (resultCreate.IsSuccess)
        {
            return default(Unit);
        }

        return ToUnknownFailure(resultCreate.FailureOrThrow());

        ValueTask<Result<Unit, StorageHttpFailure>> EnsureContainerAsync()
        {
            if (IsContainerExisted(failureUpdate))
            {
                return new(default(Unit));
            }

            var containerRequest = new StorageContainerRequest(
                hashAlgorithm: lazyHmacSha256.Value,
                baseAddress: baseAddress,
                databaseId: databaseId,
                content: new(
                    id: requestUpdate.ContainerId,
                    defaultTtlSeconds: input.ContainerTtlSeconds,
                    partitionKey: partitionKey));

            return messageHandler.SendRequestAsync(containerRequest, cancellationToken);
        }

        ValueTask<Result<Unit, StorageHttpFailure>> SendStorageItemRequestAsync(StorageItemRequest request)
            =>
            messageHandler.SendRequestAsync<Unit>(request, false, cancellationToken);

        static StorageItemWriteFailure ToUnknownFailure(StorageHttpFailure httpFailure)
            =>
            new(StorageItemWriteFailureCode.Unknown, httpFailure.FailureMessage);
    }

    private static Result<StorageItemWriteIn, StorageItemWriteFailure> ValidateKey(StorageItemWriteIn? input)
    {
        if (string.IsNullOrEmpty(input?.Key))
        {
            return new StorageItemWriteFailure(StorageItemWriteFailureCode.InvalidKey, "Key must be specified");
        }

        return input;
    }

    private static bool IsContainerExisted(StorageHttpFailure httpFailure)
    {
        return httpFailure.Headers?.FirstOrDefault(IsPartitionKeyRangeId).Value?.Any(IsNotEmpty) is true;

        static bool IsPartitionKeyRangeId(KeyValuePair<string, IEnumerable<string>> header)
            =>
            string.Equals(header.Key, "x-ms-documentdb-partitionkeyrangeid", StringComparison.InvariantCultureIgnoreCase);

        static bool IsNotEmpty(string? value)
            =>
            string.IsNullOrEmpty(value) is false;
    }
}