namespace GarageGroup.Infra.Bot.Builder;

public sealed record class StorageItemWriteIn
{
    public StorageItemWriteIn(StorageItemPath path, string key, object? value, int? containerTtlSeconds)
    {
        Path = path;
        Key = key ?? string.Empty;
        Value = value;
        ContainerTtlSeconds = containerTtlSeconds;
    }

    public StorageItemPath Path { get; }

    public string Key { get; }

    public object? Value { get; }

    public int? ContainerTtlSeconds { get; }
}