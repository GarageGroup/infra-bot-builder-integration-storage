using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public readonly record struct CosmosStorageOption
{
    private static readonly ReadOnlyDictionary<CosmosStorageContainerType, int?> emptyContainerTtlSeconds;

    static CosmosStorageOption()
    {
        var emptyDictionary = new Dictionary<CosmosStorageContainerType, int?>();
        emptyContainerTtlSeconds = new(emptyDictionary);
    }

    private readonly IReadOnlyDictionary<CosmosStorageContainerType, int?>? containerTtlSeconds;

    public CosmosStorageOption(
        [AllowNull] IReadOnlyDictionary<CosmosStorageContainerType, int?> containerTtlSeconds)
        =>
        this.containerTtlSeconds = containerTtlSeconds?.Count is not > 0 ? null : containerTtlSeconds;

    public IReadOnlyDictionary<CosmosStorageContainerType, int?> ContainerTtlSeconds
        =>
        containerTtlSeconds ?? emptyContainerTtlSeconds;
}