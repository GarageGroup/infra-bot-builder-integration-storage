using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

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
        var document = JObject.FromObject(value, jsonSerializer);
        var storageItem = new StorageItemJsonWrite(id: key.EscapeKey(), sourceId: key, document: document);

        var containerId = GetContainerId(key);
        var resourceIdUpdate = Invariant($"dbs/{databaseId}/colls/{containerId}/docs/{storageItem.Id}");

        using var clientUpdate = CreateHttpClient(verb: "PUT", resourceId: resourceIdUpdate, escapedKey: storageItem.Id);

        using var content = CreateJsonContent(storageItem);

        var responseUpdate = await clientUpdate.PutAsync(resourceIdUpdate, content, cancellationToken).ConfigureAwait(false);
        if (responseUpdate.IsSuccessStatusCode)
        {
            return;
        }

        if (responseUpdate.StatusCode is not HttpStatusCode.NotFound)
        {
            throw await CreateUnexpectedStatusCodeExceptonAsync(responseUpdate, cancellationToken).ConfigureAwait(false);
        }

        var resourceIdCreate = Invariant($"dbs/{databaseId}/colls/{containerId}");
        using var clientCreate = CreateHttpClient(verb: "POST", resourceId: resourceIdCreate, escapedKey: storageItem.Id);

        var responseCreate = await clientCreate.PostAsync(resourceIdCreate + "/docs", content, cancellationToken).ConfigureAwait(false);
        if (responseCreate.IsSuccessStatusCode is false)
        {
            throw await CreateUnexpectedStatusCodeExceptonAsync(responseCreate, cancellationToken).ConfigureAwait(false);
        }
    }
}