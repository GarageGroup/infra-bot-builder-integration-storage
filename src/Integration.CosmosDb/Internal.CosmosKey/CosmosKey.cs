using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class CosmosKey
{
    public CosmosKey(CosmosStorageContainerType containerType, [AllowNull] string containerId, [AllowNull] string itemId)
    {
        ContainerType = containerType;
        ContainerId = containerId ?? string.Empty;
        ItemId = itemId ?? string.Empty;
    }

    public CosmosStorageContainerType ContainerType { get; }

    public string ContainerId { get; }

    public string ItemId { get; }
}