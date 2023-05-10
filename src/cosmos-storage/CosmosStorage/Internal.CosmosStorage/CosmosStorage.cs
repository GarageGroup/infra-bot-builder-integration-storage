using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed partial class CosmosStorage : ICosmosStorage
{
    private static readonly Regex ItemPathRegex;

    static CosmosStorage()
        =>
        ItemPathRegex = new(
            "^([^/\\?#*]+)/(users|conversations)/([^/\\?#*]+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static CosmosStorage Create(IFunc<ICosmosApi> cosmosApiProvider, CosmosStorageOption option)
        =>
        new(
            cosmosApiProvider ?? throw new ArgumentNullException(nameof(cosmosApiProvider)), option);

    public static CosmosStorage Create(IFunc<ICosmosApi> cosmosApiProvider)
        =>
        new(
            cosmosApiProvider ?? throw new ArgumentNullException(nameof(cosmosApiProvider)), default);

    private readonly Lazy<ICosmosApi> lazyCosmosApi;

    private readonly IReadOnlyDictionary<CosmosStorageContainerType, int?> containerTtlSeconds;

    private bool disposed;

    private CosmosStorage(IFunc<ICosmosApi> cosmosApiProvider, CosmosStorageOption option)
    {
        lazyCosmosApi = new Lazy<ICosmosApi>(cosmosApiProvider.Invoke, LazyThreadSafetyMode.ExecutionAndPublication);
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
        itemType switch
        {
            StorageItemType.UserState => GetTtlSeconds(CosmosStorageContainerType.UserState),
            StorageItemType.ConversationState => GetTtlSeconds(CosmosStorageContainerType.ConversationState),
            _ => GetTtlSeconds(CosmosStorageContainerType.BotStorage)
        };

    private int? GetTtlSeconds(CosmosStorageContainerType itemType)
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

    private static StorageItemLockPath ParseItemLockPath(string source)
    {
        var match = ItemPathRegex.Match(source);

        if (match.Success is false)
        {
            return new(string.Empty, source);
        }

        return new(
            channelId: match.Groups[1].Value,
            itemId: $"{match.Groups[2].Value}:{match.Groups[3].Value}");
    }

    private static T InnerPipeSelf<T>(T item)
        =>
        item;
}