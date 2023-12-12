using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class CosmosStorageOption
{
    private static readonly ReadOnlyDictionary<CosmosStorageContainerType, int?> EmptyContainerTtlSeconds;

    static CosmosStorageOption()
    {
        var emptyDictionary = new Dictionary<CosmosStorageContainerType, int?>();
        EmptyContainerTtlSeconds = new(emptyDictionary);
    }

    public CosmosStorageOption(
        Uri baseAddress,
        string databaseId,
        [AllowNull] string masterKey,
        [AllowNull] IReadOnlyDictionary<CosmosStorageContainerType, int?> containerTtlSeconds,
        [AllowNull] IReadOnlyList<string> pingChannels = null)
    {
        BaseAddress = baseAddress;
        DatabaseId = databaseId.OrEmpty();
        MasterKey = masterKey.OrNullIfEmpty();
        ContainerTtlSeconds = containerTtlSeconds ?? EmptyContainerTtlSeconds;
        PingChannels = pingChannels ?? Array.Empty<string>();
    }

    public Uri BaseAddress { get; }

    public string DatabaseId { get; }

    public string? MasterKey { get; }

    public IReadOnlyDictionary<CosmosStorageContainerType, int?> ContainerTtlSeconds { get; }

    public IReadOnlyList<string> PingChannels { get; }

    public int? MaxDegreeOfParallelism { get; init; }
}