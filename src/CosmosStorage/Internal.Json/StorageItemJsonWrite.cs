using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class StorageItemJsonWrite
{
    public StorageItemJsonWrite(string id, string key, JObject? value)
    {
        Id = id.OrEmpty();
        Key = key.OrEmpty();
        Value = value;
    }

    [JsonProperty(StorageItemJsonProperty.Id)]
    public string Id { get; }

    [JsonProperty(StorageItemJsonProperty.Key)]
    public string Key { get; }

    [JsonProperty(StorageItemJsonProperty.Value)]
    public JObject? Value { get; init; }
}