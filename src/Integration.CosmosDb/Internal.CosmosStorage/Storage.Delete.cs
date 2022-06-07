using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        if (keys is null || keys.Length is default(int))
        {
            return Task.CompletedTask;
        }

        return Task.WhenAll(keys.Select(DeleteItemAsync));
        Task DeleteItemAsync(string key) => InnerDeleteItemAsync(key, cancellationToken);
    }

    private async Task InnerDeleteItemAsync(string key, CancellationToken cancellationToken)
    {
        var cosmosKey = key.ParseKey();
        var resourceId = $"dbs/{databaseId}/colls/{cosmosKey.ContainerId}/docs/{cosmosKey.ItemId}";

        using var client = CreateHttpClient(
            verb: "DELETE",
            resourceId: resourceId,
            resourceType: ItemResourceType,
            escapedKey: cosmosKey.ItemId);

        var response = await client.DeleteAsync(resourceId, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode is false && response.StatusCode is not HttpStatusCode.NotFound)
        {
            throw await CreateUnexpectedStatusCodeExceptonAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }
}