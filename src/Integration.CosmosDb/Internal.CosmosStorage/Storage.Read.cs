using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task<IDictionary<string, object?>> ReadAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<IDictionary<string, object?>>(cancellationToken);
        }

        var notEmptyKeys = keys?.Where(IsNotEmpty).ToArray();
        if (notEmptyKeys is null || notEmptyKeys.Length is default(int))
        {
            return Task.FromResult<IDictionary<string, object?>>(new Dictionary<string, object?>());
        }

        return InnerReadAsync(notEmptyKeys, cancellationToken);

        static bool IsNotEmpty(string value) => string.IsNullOrEmpty(value) is false;
    }

    private async Task<IDictionary<string, object?>> InnerReadAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken)
    {
        var items = await Task.WhenAll(keys.Select(ReadByKeyAsync));
        return items.Where(IsNotNull).ToDictionary(GetKey, GetValue);

        Task<StorageItemJsonRead?> ReadByKeyAsync(string key)
            =>
            InnerReadItemAsync(key, cancellationToken);

        static bool IsNotNull([NotNullWhen(true)] StorageItemJsonRead? storageItem)
            =>
            storageItem is not null;

        static string GetKey(StorageItemJsonRead? storageItem)
            =>
            storageItem?.Key ?? string.Empty;

        static object? GetValue(StorageItemJsonRead? storageItem)
            =>
            storageItem?.Value?.ToObject<object>(jsonSerializer);
    }

    private async Task<StorageItemJsonRead?> InnerReadItemAsync(string key, CancellationToken cancellationToken)
    {
        var (containerId, _, itemId) = key.ParseKey();
        var resourceId = $"dbs/{databaseId}/colls/{containerId}/docs/{itemId}";

        using var client = CreateHttpClient(
            verb: "GET",
            resourceId: resourceId,
            resourceType: ItemResourceType,
            escapedKey: itemId);

        var response = await client.GetAsync(resourceId, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return default;
        }

        if (response.IsSuccessStatusCode is false)
        {
            throw await CreateUnexpectedStatusCodeExceptonAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(body))
        {
            return default;
        }

        return JsonConvert.DeserializeObject<StorageItemJsonRead>(body);
    }
}