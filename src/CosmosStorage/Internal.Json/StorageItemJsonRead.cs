using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GarageGroup.Infra.Bot.Builder;

internal readonly record struct StorageItemJsonRead
{
    [JsonProperty(StorageItemJsonProperty.Key)]
    public string? Key { get; init; }

    [JsonProperty(StorageItemJsonProperty.Value)]
    public JObject? Value { get; init; }
}