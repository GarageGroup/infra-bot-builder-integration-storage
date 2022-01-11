using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StorageItemSetJsonRead
{
    [JsonProperty("Documents")]
    public StorageItemJsonRead[]? Documents { get; init; }
}