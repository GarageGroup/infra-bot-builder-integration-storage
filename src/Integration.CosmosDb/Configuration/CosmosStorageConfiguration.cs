using System;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class CosmosStorageConfiguration
{
    public CosmosStorageConfiguration(
        Uri baseAddress,
        string masterKey,
        string databaseId,
        string userStateContainerId,
        string defaultContainerId)
    {
        BaseAddress = baseAddress;
        MasterKey = masterKey ?? string.Empty;
        DatabaseId = databaseId ?? string.Empty;
        UserStateContainerId = userStateContainerId ?? string.Empty;
        DefaultContainerId = defaultContainerId ?? string.Empty;
    }

    public Uri BaseAddress { get; }

    public string MasterKey { get; }

    public string DatabaseId { get; }

    public string UserStateContainerId { get; }

    public string DefaultContainerId { get; }
}