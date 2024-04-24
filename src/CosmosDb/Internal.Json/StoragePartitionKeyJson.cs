using System;
using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class StoragePartitionKeyJson
{
    internal StoragePartitionKeyJson(string[] paths, string kind, int version)
    {
        Paths = paths ?? [];
        Kind = kind.OrEmpty();
        Version = version;
    }

    [JsonProperty("paths")]
    public string[] Paths { get; }

    [JsonProperty("kind")]
    public string Kind { get; }

    [JsonProperty(nameof(Version))]
    public int Version { get; }
}