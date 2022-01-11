using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StorageQueryParameterJson
{
    internal StorageQueryParameterJson(string name, string? value)
    {
        Name = name ?? string.Empty;
        Value = value;
    }

    [JsonProperty("name")]
    public string Name { get; }

    [JsonProperty("value")]
    public string? Value { get; }
}