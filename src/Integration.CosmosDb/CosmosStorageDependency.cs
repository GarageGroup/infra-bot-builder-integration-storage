using System;
using System.Net.Http;
using Microsoft.Bot.Builder;
using PrimeFuncPack;

namespace GGroupp.Infra.Bot.Builder;

public static class CosmosStorageDependency
{
    public static Dependency<IStorage> UseCosmosStorage(
        this Dependency<HttpMessageHandler> dependency, Func<IServiceProvider, CosmosStorageConfiguration> configurationResolver)
    {
        _ = dependency ?? throw new ArgumentNullException(nameof(dependency));
        _ = configurationResolver ?? throw new ArgumentNullException(nameof(configurationResolver));

        return dependency.With(configurationResolver).Fold<IStorage>(CosmosStorage.Create);
    }

    public static Dependency<IStorage> UseCosmosStorage(
        this Dependency<HttpMessageHandler, CosmosStorageConfiguration> dependency)
    {
        _ = dependency ?? throw new ArgumentNullException(nameof(dependency));

        return dependency.Fold<IStorage>(CosmosStorage.Create);
    }
}