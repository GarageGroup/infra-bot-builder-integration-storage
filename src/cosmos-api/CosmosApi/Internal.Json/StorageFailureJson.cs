using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal readonly record struct StorageFailureJson
{
    [JsonProperty("message")]
    public string? Message { get; init; }
}