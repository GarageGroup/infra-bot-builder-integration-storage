using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        if (disposed)
        {
            throw CreateDisposedException();
        }

        if (keys?.Length is not > 0)
        {
            return Task.CompletedTask;
        }

        if (semaphore is null)
        {
            return DeleteAllAsync(keys, cancellationToken);
        }

        return semaphore.InvokeAsync(DeleteAllAsync, keys, cancellationToken);

        Task DeleteAllAsync(string[] keys, CancellationToken cancellationToken)
            =>
            Task.WhenAll(keys.Select(DeleteItemAsync));

        Task DeleteItemAsync(string key)
            =>
            InnerDeleteItemAsync(key.OrEmpty(), cancellationToken);
    }

    private async Task InnerDeleteItemAsync(string key, CancellationToken cancellationToken)
    {
        var itemPath = StorageItemParser.ParseItemPath(key);

        var containerId = itemPath.GetContainerId();
        var itemId = itemPath.ItemId.EscapeItemId();

        var resourceId = $"dbs/{option.DatabaseId}/colls/{containerId}/docs/{itemId}";

        using var client = CreateHttpClient(escapedKey: itemId);

        var response = await client.GetResponseAsync<StorageItemJsonWrite>(
            method: HttpMethod.Delete,
            hashAlgorithm: hashAlgorithm,
            resourceId: resourceId,
            resourceType: ItemResourceType,
            requestUri: resourceId,
            contentJson: null,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (response.IsSuccessStatusCode is false && response.StatusCode is not HttpStatusCode.NotFound)
        {
            throw await GetUnexpectedStatusCodeExceptonAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }
}