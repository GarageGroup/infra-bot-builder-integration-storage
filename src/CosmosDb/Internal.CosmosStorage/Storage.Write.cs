using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task WriteAsync(IDictionary<string, object?> changes, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        if (disposed)
        {
            throw CreateDisposedException();
        }

        if (changes?.Count is not > 0)
        {
            return Task.CompletedTask;
        }

        if (semaphore is null)
        {
            return InnerWriteAsync(changes, cancellationToken);
        }

        return semaphore.InvokeAsync(InnerWriteAsync, changes, cancellationToken);
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
        var itemPath = StorageItemParser.ParseItemPath(key);

        var containerId = itemPath.GetContainerId();
        var itemId = itemPath.ItemId.EscapeItemId();

        var jValue = JObject.FromObject(value, JsonSerializer);
        var storageItem = new StorageItemJsonWrite(id: itemPath.ItemId, key: key, value: jValue);

        var resourceIdUpdate = $"dbs/{option.DatabaseId}/colls/{containerId}/docs/{itemId}";

        using var client = CreateHttpClient(escapedKey: itemId);

        var responseUpdate = await client.GetResponseAsync(
            method: HttpMethod.Put,
            hashAlgorithm: hashAlgorithm,
            resourceId: resourceIdUpdate,
            resourceType: ItemResourceType,
            requestUri: resourceIdUpdate,
            contentJson: storageItem,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (responseUpdate.IsSuccessStatusCode)
        {
            return;
        }

        if (responseUpdate.StatusCode is not HttpStatusCode.NotFound)
        {
            throw await GetUnexpectedStatusCodeExceptonAsync(responseUpdate, cancellationToken).ConfigureAwait(false);
        }

        if (IsContainerExisted(responseUpdate) is false)
        {
            await CreateContainerAsync(itemPath, cancellationToken).ConfigureAwait(false);
        }

        var resourceIdCreate = $"dbs/{option.DatabaseId}/colls/{containerId}";

        var responseCreate = await client.GetResponseAsync(
            method: HttpMethod.Post,
            hashAlgorithm: hashAlgorithm,
            resourceId: resourceIdCreate,
            resourceType: ItemResourceType,
            requestUri: resourceIdCreate + "/docs",
            contentJson: storageItem,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (responseCreate.IsSuccessStatusCode)
        {
            return;
        }

        if (responseCreate.StatusCode is not HttpStatusCode.Conflict)
        {
            throw await GetUnexpectedStatusCodeExceptonAsync(responseCreate, cancellationToken).ConfigureAwait(false);
        }

        var responseSecondUpdate = await client.GetResponseAsync(
            method: HttpMethod.Put,
            hashAlgorithm: hashAlgorithm,
            resourceId: resourceIdUpdate,
            resourceType: ItemResourceType,
            requestUri: resourceIdUpdate,
            contentJson: storageItem,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (responseSecondUpdate.IsSuccessStatusCode is false)
        {
            throw await GetUnexpectedStatusCodeExceptonAsync(responseSecondUpdate, cancellationToken).ConfigureAwait(false);
        }
    }
}