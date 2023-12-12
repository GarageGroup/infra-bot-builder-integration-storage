namespace GarageGroup.Infra.Bot.Builder;

internal interface IStorageContainerPath
{
    StorageItemType ItemType { get; }

    string ChannelId { get; }
}