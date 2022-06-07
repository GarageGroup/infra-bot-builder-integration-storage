using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task WriteAsync(IDictionary<string, object?> changes, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

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
        var cosmosKey = key.ParseKey();

        var jValue = JObject.FromObject(value, jsonSerializer);
        var storageItem = new StorageItemJsonWrite(id: cosmosKey.ItemId, key: key, value: jValue);

        using var content = CreateJsonContent(storageItem);
        var resourceIdUpdate = $"dbs/{databaseId}/colls/{cosmosKey.ContainerId}/docs/{cosmosKey.ItemId}";

        using var clientUpdate = CreateHttpClient(
            verb: "PUT",
            resourceId: resourceIdUpdate,
            resourceType: ItemResourceType,
            escapedKey: cosmosKey.ItemId);

        var responseUpdate = await clientUpdate.PutAsync(resourceIdUpdate, content, cancellationToken).ConfigureAwait(false);
        if (responseUpdate.IsSuccessStatusCode)
        {
            return;
        }

        if (responseUpdate.StatusCode is not HttpStatusCode.NotFound)
        {
            throw await CreateUnexpectedStatusCodeExceptonAsync(responseUpdate, cancellationToken).ConfigureAwait(false);
        }

        if (HasContainerExisted(responseUpdate) is false)
        {
            var containerTtlSeconds = GetTtlSeconds(cosmosKey.ContainerType);
            await InnerCreateContainer(cosmosKey.ContainerId, containerTtlSeconds, cancellationToken).ConfigureAwait(false);
        }

        var resourceIdCreate = $"dbs/{databaseId}/colls/{cosmosKey.ContainerId}";
        using var clientCreate = CreateHttpClient(
            verb: "POST",
            resourceId: resourceIdCreate,
            resourceType: ItemResourceType,
            escapedKey: cosmosKey.ItemId);

        var responseCreate = await clientCreate.PostAsync(resourceIdCreate + "/docs", content, cancellationToken).ConfigureAwait(false);
        if (responseCreate.IsSuccessStatusCode)
        {
            return;
        }

        if (responseCreate.StatusCode is not HttpStatusCode.Conflict)
        {
            throw await CreateUnexpectedStatusCodeExceptonAsync(responseCreate, cancellationToken).ConfigureAwait(false);
        }

        var responseSecondUpdate = await clientUpdate.PutAsync(resourceIdUpdate, content, cancellationToken).ConfigureAwait(false);
        if (responseSecondUpdate.IsSuccessStatusCode is false)
        {
            throw await CreateUnexpectedStatusCodeExceptonAsync(responseSecondUpdate, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool HasContainerExisted(HttpResponseMessage response)
        =>
        response.Headers.TryGetValues("x-ms-documentdb-partitionkeyrangeid", out var values) && values.Any(v => string.IsNullOrEmpty(v) is false);

    private async Task InnerCreateContainer(string containerId, int? ttlSeconds, CancellationToken cancellationToken)
    {
        var resourceId = $"dbs/{databaseId}";
        using var client = CreateHttpClient(verb: "POST", resourceId: resourceId, resourceType: ContainerResourceType);

        var container = new StorageContainerJsonWrite(
            id: containerId,
            defaultTtlSeconds: ttlSeconds,
            partitionKey: partitionKey);

        using var content = CreateJsonContent(container);
        var response = await client.PostAsync(resourceId + "/colls", content, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode is false && response.StatusCode is not HttpStatusCode.Conflict)
        {
            throw await CreateUnexpectedStatusCodeExceptonAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }
}