namespace GarageGroup.Infra.Bot.Builder;

public sealed record class StorageItemLockPath
{
    public StorageItemLockPath(string channelId, string itemId)
    {
        ChannelId = channelId ?? string.Empty;
        ItemId = itemId ?? string.Empty;
    }

    public string ChannelId { get; }

    public string ItemId { get; }
}