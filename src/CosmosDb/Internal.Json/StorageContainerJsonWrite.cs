using System;
using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class StorageContainerJsonWrite
{
    public StorageContainerJsonWrite(string id, int? defaultTtlSeconds, StoragePartitionKeyJson partitionKey)
    {
        Id = id.OrEmpty();
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