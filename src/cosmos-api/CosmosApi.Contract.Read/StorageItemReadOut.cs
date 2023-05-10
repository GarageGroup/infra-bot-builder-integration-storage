using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public readonly record struct StorageItemReadOut
{
    private readonly string? key;

    public StorageItemReadOut([AllowNull] string key, IStorageValueRead? value)
    {
        this.key = string.IsNullOrEmpty(key) ? null : key;
        Value = value;
    }

    public string Key => key ?? string.Empty;

    public IStorageValueRead? Value { get; }
}