using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public ValueTask<Result<Unit, Failure<Unit>>> PingAsync(Unit input, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<Unit, Failure<Unit>>>(cancellationToken);
        }

        if (disposed)
        {
            throw CreateDisposedException();
        }

        if (option.PingChannels.Count is 0)
        {
            return new(Result.Success(input));
        }

        return InnerPingAsync(cancellationToken);
    }

    private async ValueTask<Result<Unit, Failure<Unit>>> InnerPingAsync(CancellationToken cancellationToken)
    {
        var containers = option.PingChannels.SelectMany(GetContainerPathes).ToArray();
        var failures = new ConcurrentBag<(int Index, Failure<Unit> Failure)>();

        await Parallel.ForEachAsync(Enumerable.Range(0, containers.Length), InnerPingChannelAsync).ConfigureAwait(false);

        if (failures.IsEmpty is false)
        {
            return failures.OrderBy(GetIndex).First().Failure;
        }

        return Result.Success<Unit>(default);

        async ValueTask InnerPingChannelAsync(int index, CancellationToken cancellationToken)
        {
            var result = await PingChannelAsync(containers[index], cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                failures.Add((index, result.FailureOrThrow()));
            }
        }

        static IEnumerable<StorageContainerPath> GetContainerPathes(string channel)
            =>
            new StorageContainerPath[]
            {
                new(StorageItemType.UserState, channel),
                new(StorageItemType.ConversationState, channel)
            };

        static int GetIndex((int Index, Failure<Unit>) item)
            =>
            item.Index;
    }

    private async ValueTask<Result<Unit, Failure<Unit>>> PingChannelAsync(
        StorageContainerPath containerPath, CancellationToken cancellationToken)
    {
        var containerId = containerPath.GetContainerId();
        var resourceId = $"dbs/{option.DatabaseId}/colls/{containerId}";

        using var client = CreateHttpClient();

        var response = await client.GetResponseAsync<object>(
            method: HttpMethod.Get,
            hashAlgorithm: hashAlgorithm,
            resourceId: resourceId,
            resourceType: ContainerResourceType,
            requestUri: resourceId,
            contentJson: default,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return Result.Success<Unit>(default);
        }

        if (response.StatusCode is not HttpStatusCode.NotFound)
        {
            var unexpectedCodeExcepton = await GetUnexpectedStatusCodeExceptonAsync(response, cancellationToken).ConfigureAwait(false);
            return unexpectedCodeExcepton.ToFailure(unexpectedCodeExcepton.Message);
        }

        return await InnerCreateContainerOrFailureAsync().ConfigureAwait(false);

        async ValueTask<Result<Unit, Failure<Unit>>> InnerCreateContainerOrFailureAsync()
        {
            try
            {
                await CreateContainerAsync(containerPath, cancellationToken).ConfigureAwait(false);
                return Result.Success<Unit>(default);
            }
            catch (InvalidOperationException ex)
            {
                return ex.ToFailure(ex.Message);
            }
        }
    }
}