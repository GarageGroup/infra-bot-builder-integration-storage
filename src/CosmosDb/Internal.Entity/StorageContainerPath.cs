using System;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class StorageContainerPath : IStorageContainerPath
{
    public StorageContainerPath(StorageItemType itemType, string channelId)
    {
        ItemType = itemType;
        ChannelId = channelId.OrEmpty();
    }

    public StorageItemType ItemType { get; }

    public string ChannelId { get; }
}
