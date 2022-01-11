using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StorageItemJsonWrite
{
    internal StorageItemJsonWrite(string id, string sourceId, JObject document)
    {
        Id = id;
        SourceId = sourceId;
        Document = document;
    }

    [JsonProperty(StorageItemJsonProperty.Id)]
    public string Id { get; }

    [JsonProperty(StorageItemJsonProperty.SourceId)]
    public string SourceId { get; }

    [JsonProperty(StorageItemJsonProperty.Document)]
    public JObject Document { get; }
}