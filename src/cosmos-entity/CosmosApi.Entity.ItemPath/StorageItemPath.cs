using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class StorageItemPath
{
    public StorageItemPath(StorageItemType itemType, [AllowNull] string channelId, string userId)
    {
        ItemType = itemType;
        ChannelId = channelId ?? string.Empty;
        UserId = userId ?? string.Empty;
    }

    public StorageItemType ItemType { get; }

    public string ChannelId { get; }

    public string UserId { get; }
}