using System;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class StorageItemLockPath
{
    public StorageItemLockPath(string channelId, string itemId)
    {
        ChannelId = channelId.OrEmpty();
        ItemId = itemId.OrEmpty();
    }

    public string ChannelId { get; }

    public string ItemId { get; }
}