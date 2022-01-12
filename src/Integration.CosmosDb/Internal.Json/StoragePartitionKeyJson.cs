using System;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StoragePartitionKeyJson
{
    internal StoragePartitionKeyJson(
        string[] paths,
        string kind,
        int version)
    {
        Paths = paths ?? Array.Empty<string>();
        Kind = kind ?? string.Empty;
        Version = version;
    }

    [JsonProperty("paths")]
    public string[] Paths { get; }

    [JsonProperty("kind")]
    public string Kind { get; }

    [JsonProperty("Version")]
    public int Version { get; }
}