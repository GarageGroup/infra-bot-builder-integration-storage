using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StorageItemJsonRead
{
    [JsonProperty(StorageItemJsonProperty.SourceId)]
    public string? SourceId { get; init; }

    [JsonProperty(StorageItemJsonProperty.Document)]
    public JObject? Document { get; init; }
}