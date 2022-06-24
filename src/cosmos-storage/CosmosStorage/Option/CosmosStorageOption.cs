using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public readonly record struct CosmosStorageOption
{
    private static readonly ReadOnlyDictionary<StorageItemType, int?> emptyContainerTtlSeconds;

    static CosmosStorageOption()
    {
        var emptyDictionary = new Dictionary<StorageItemType, int?>();
        emptyContainerTtlSeconds = new(emptyDictionary);
    }

    private readonly IReadOnlyDictionary<StorageItemType, int?>? containerTtlSeconds;

    public CosmosStorageOption(
        [AllowNull] IReadOnlyDictionary<StorageItemType, int?> containerTtlSeconds)
        =>
        this.containerTtlSeconds = containerTtlSeconds?.Count is not > 0 ? null : containerTtlSeconds;

    public IReadOnlyDictionary<StorageItemType, int?> ContainerTtlSeconds
        =>
        containerTtlSeconds ?? emptyContainerTtlSeconds;
}