using System;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class StorageItemPath
{
    public StorageItemPath(StorageItemType itemType, string channelId, string itemId)
    {
        ItemType = itemType;
        ChannelId = channelId.OrEmpty();
        ItemId = itemId.OrEmpty();
    }

    public StorageItemType ItemType { get; }

    public string ChannelId { get; }

    public string ItemId { get; }
}