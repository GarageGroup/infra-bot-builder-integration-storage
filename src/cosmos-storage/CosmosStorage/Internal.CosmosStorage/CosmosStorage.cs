using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace GGroupp.Infra.Bot.Builder;

internal sealed partial class CosmosStorage<TCosmosApi> : ICosmosStorage
    where TCosmosApi : class, IStorageItemReadSupplier, IStorageItemWriteSupplier, IStorageItemDeleteSupplier
{
    private static readonly Regex ItemPathRegex;

    static CosmosStorage()
        =>
        ItemPathRegex = new(
            "^([^/\\?#*]+)/(users|conversations)/([^/\\?#*]+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static CosmosStorage<TCosmosApi> Create(IFunc<TCosmosApi> cosmosApiProvider, CosmosStorageOption option)
        =>
        new(
            cosmosApiProvider ?? throw new ArgumentNullException(nameof(cosmosApiProvider)), option);

    public static CosmosStorage<TCosmosApi> Create(IFunc<TCosmosApi> cosmosApiProvider)
        =>
        new(
            cosmosApiProvider ?? throw new ArgumentNullException(nameof(cosmosApiProvider)), default);

    private readonly Lazy<TCosmosApi> lazyCosmosApi;

    private readonly IReadOnlyDictionary<StorageItemType, int?> containerTtlSeconds;

    private bool disposed;

    private CosmosStorage(IFunc<TCosmosApi> cosmosApiProvider, CosmosStorageOption option)
    {
        lazyCosmosApi = new Lazy<TCosmosApi>(cosmosApiProvider.Invoke, LazyThreadSafetyMode.ExecutionAndPublication);
        containerTtlSeconds = option.ContainerTtlSeconds;
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException("CosmosStorage has already been disposed");
        }
    }

    private int? GetTtlSeconds(StorageItemType itemType)
        =>
        containerTtlSeconds.TryGetValue(itemType, out var ttl) ? ttl : null;

    private static StorageItemPath ParseItemPath(string source)
    {
        var match = ItemPathRegex.Match(source);

        if (match.Success is false)
        {
            return new(
                itemType: default,
                channelId: default,
                userId: source);
        }

        return new(
            itemType: match.Groups[2].Value[0] is 'u' ? StorageItemType.UserState : StorageItemType.ConversationState,
            channelId: match.Groups[1].Value,
            userId: match.Groups[3].Value);
    }

    private static T InnerPipeSelf<T>(T item)
        =>
        item;
}