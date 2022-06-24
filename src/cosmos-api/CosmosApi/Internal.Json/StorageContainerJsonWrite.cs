using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StorageContainerJsonWrite
{
    public StorageContainerJsonWrite(string id, int? defaultTtlSeconds, StoragePartitionKeyJson partitionKey)
    {
        Id = id ?? string.Empty;
        DefaultTtlSeconds = defaultTtlSeconds;
        PartitionKey = partitionKey;
    }

    [JsonProperty("id")]
    public string Id { get; }

    [JsonProperty("defaultTtl")]
    public int? DefaultTtlSeconds { get; }

    [JsonProperty("partitionKey")]
    public StoragePartitionKeyJson PartitionKey { get; }
}