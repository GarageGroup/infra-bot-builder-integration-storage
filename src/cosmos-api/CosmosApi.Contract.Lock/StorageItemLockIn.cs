namespace GarageGroup.Infra.Bot.Builder;

public sealed record class StorageItemLockIn
{
    public StorageItemLockIn(StorageItemLockPath path)
        =>
        Path = path;

    public StorageItemLockPath Path { get; }

    public int? ContainerTtlSeconds { get; init; }
}