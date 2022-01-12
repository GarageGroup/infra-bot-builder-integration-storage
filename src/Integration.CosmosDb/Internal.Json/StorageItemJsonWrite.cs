using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StorageItemJsonWrite
{
    internal StorageItemJsonWrite(string id, string key, JObject value)
    {
        Id = id ?? string.Empty;
        Key = key ?? string.Empty;
        Value = value;
    }

    [JsonProperty(StorageItemJsonProperty.Id)]
    public string Id { get; }

    [JsonProperty(StorageItemJsonProperty.Key)]
    public string Key { get; }

    [JsonProperty(StorageItemJsonProperty.Value)]
    public JObject Value { get; }
}