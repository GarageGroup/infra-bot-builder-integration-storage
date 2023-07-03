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
        string masterKey,
        string databaseId,
        [AllowNull] IReadOnlyDictionary<CosmosStorageContainerType, int?> containerTtlSeconds)
    {
        BaseAddress = baseAddress;
        MasterKey = masterKey.OrEmpty();
        DatabaseId = databaseId.OrEmpty();
        ContainerTtlSeconds = containerTtlSeconds ?? EmptyContainerTtlSeconds;
    }

    public Uri BaseAddress { get; }

    public string MasterKey { get; }

    public string DatabaseId { get; }

    public IReadOnlyDictionary<CosmosStorageContainerType, int?> ContainerTtlSeconds { get; }

    public int? MaxDegreeOfParallelism { get; init; }
}