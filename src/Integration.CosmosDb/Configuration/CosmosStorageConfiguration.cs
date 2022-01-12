using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class CosmosStorageConfiguration
{
    private static readonly ReadOnlyDictionary<CosmosStorageContainerType, int?> emptyContainerTtlSeconds;

    static CosmosStorageConfiguration()
    {
        var emptyDictiobary = new Dictionary<CosmosStorageContainerType, int?>();
        emptyContainerTtlSeconds = new(emptyDictiobary);
    }

    public CosmosStorageConfiguration(
        Uri baseAddress,
        string masterKey,
        string databaseId,
        IReadOnlyDictionary<CosmosStorageContainerType, int?>? containerTtlSeconds)
    {
        BaseAddress = baseAddress;
        MasterKey = masterKey ?? string.Empty;
        DatabaseId = databaseId ?? string.Empty;
        ContainerTtlSeconds = containerTtlSeconds ?? emptyContainerTtlSeconds;
    }

    public Uri BaseAddress { get; }

    public string MasterKey { get; }

    public string DatabaseId { get; }

    public IReadOnlyDictionary<CosmosStorageContainerType, int?> ContainerTtlSeconds { get; }
}