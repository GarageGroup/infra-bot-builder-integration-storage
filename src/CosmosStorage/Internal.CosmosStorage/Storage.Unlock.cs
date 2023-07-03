using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task UnlockAsync(string key, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        if (disposed)
        {
            throw CreateDisposedException();
        }

        return semaphore.InvokeAsync(InnerUnlockAsync, key.OrEmpty(), cancellationToken);
    }

    private async Task InnerUnlockAsync(string key, CancellationToken cancellationToken)
    {
        var itemPath = StorageItemParser.ParseItemLockPath(key);

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