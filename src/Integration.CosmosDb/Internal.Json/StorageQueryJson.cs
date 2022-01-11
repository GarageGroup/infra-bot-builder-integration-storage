using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StorageQueryJson
{
    internal StorageQueryJson(string query, [AllowNull] StorageQueryParameterJson[] parameters)
    {
        Query = query ?? string.Empty;
        Parameters = parameters ?? Array.Empty<StorageQueryParameterJson>();
    }

    [JsonProperty("query")]
    public string Query { get; }

    [JsonProperty("parameters")]
    public StorageQueryParameterJson[] Parameters { get; }
}