using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StorageItemJsonRead
{
    [JsonProperty(StorageItemJsonProperty.Key)]
    public string? Key { get; init; }

    [JsonProperty(StorageItemJsonProperty.Value)]
    public JObject? Value { get; init; }
}