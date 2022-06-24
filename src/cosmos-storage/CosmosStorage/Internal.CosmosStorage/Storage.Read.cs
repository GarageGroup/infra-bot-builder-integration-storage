using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class CosmosStorage<TCosmosApi>
{
    public Task<IDictionary<string, object?>> ReadAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

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
        var items = await Task.WhenAll(keys.Select(ReadByKeyAsync)).ConfigureAwait(false);
        return items.Where(IsNotNull).ToDictionary(GetKey, GetValue);

        Task<StorageItemReadOut> ReadByKeyAsync(string key)
            =>
            InnerReadItemAsync(key, cancellationToken);

        static bool IsNotNull(StorageItemReadOut storageItem)
            =>
            storageItem.Value is not null;

        static string GetKey(StorageItemReadOut storageItem)
            =>
            storageItem.Key;

        static object? GetValue(StorageItemReadOut storageItem)
            =>
            storageItem.Value?.ToObject();
    }

    private async Task<StorageItemReadOut> InnerReadItemAsync(string key, CancellationToken cancellationToken)
    {
        var itemPath = ParseItemPath(key);
        var result = await lazyCosmosApi.Value.ReadItemAsync(itemPath, cancellationToken).ConfigureAwait(false);

        return result.Fold(InnerPipeSelf, MapFailureOrThrow);
    }

    private static StorageItemReadOut MapFailureOrThrow(StorageItemReadFailure failure)
    {
        if (failure.FailureCode is StorageItemReadFailureCode.NotFound)
        {
            return default;
        }

        throw new InvalidOperationException(failure.FailureMessage);
    }
}