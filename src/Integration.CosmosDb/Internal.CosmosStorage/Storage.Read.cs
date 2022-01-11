using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.FormattableString;

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
        var dictionaries = await Task.WhenAll(keys.GroupBy(GetContainerId).Select(InnerReadContainerAsync));
        return dictionaries.SelectMany(d => d).ToDictionary(kv => kv.Key, kv => kv.Value);

        Task<IDictionary<string, object?>> InnerReadContainerAsync(IGrouping<string, string> group)
            =>
            InnerReadAsync(group.Key, group.ToArray(), cancellationToken);
    }

    private async Task<IDictionary<string, object?>> InnerReadAsync(
        string containerId, IReadOnlyCollection<string> keys, CancellationToken cancellationToken)
    {
        var resourceId = Invariant($"dbs/{databaseId}/colls/{containerId}");

        var query = BuildQuery(keys);
        using var content = CreateJsonContent(query);
        content.Headers.ContentType = new("application/query+json");

        var result = new Dictionary<string, object?>();
        string? continuationToken = default;

        do
        {
            continuationToken = await InnerReadItemsAsync().ConfigureAwait(false);
        }
        while (string.IsNullOrEmpty(continuationToken) is false);

        return result;

        async Task<string?> InnerReadItemsAsync()
        {
            using var client = CreateQueryHttpClient(verb: "POST", resourceId: resourceId, continuationToken: continuationToken);
            var response = await client.PostAsync(resourceId + "/docs", content, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode is false)
            {
                throw await CreateUnexpectedStatusCodeExceptonAsync(response, cancellationToken).ConfigureAwait(false);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(body))
            {
                return GetContinuationToken(response);
            }

            var storageItems = JsonConvert.DeserializeObject<StorageItemSetJsonRead>(body)?.Documents;
            if (storageItems is null || storageItems.Length is default(int))
            {
                return GetContinuationToken(response);
            }

            foreach (var storageItem in storageItems)
            {
                result[storageItem.SourceId ?? string.Empty] = storageItem?.Document?.ToObject<object>(jsonSerializer);
            }

            return GetContinuationToken(response);
        }
    }

    private static StorageQueryJson BuildQuery(IReadOnlyCollection<string> keys)
    {
        var names = string.Join(",", Enumerable.Range(0, keys.Count).Select(CreateParameterName));

        return new(
            query: $"SELECT c.{StorageItemJsonProperty.SourceId}, c.{StorageItemJsonProperty.Document} FROM c WHERE c.{StorageItemJsonProperty.Id} IN ({names})",
            parameters: keys.Select(CreateParameterJson).ToArray());

        static StorageQueryParameterJson CreateParameterJson(string key, int index)
            =>
            new(
                name: CreateParameterName(index),
                value: key.EscapeKey());

        static string CreateParameterName(int index)
            =>
            Invariant($"@id{index}");
    }
}