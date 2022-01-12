using System;
using System.Net.Http;
using Microsoft.Bot.Builder;
using PrimeFuncPack;

namespace GGroupp.Infra.Bot.Builder;

public static class CosmosStorageDependency
{
    public static Dependency<IStorage> UseCosmosStorage<THttpMessageHandler>(
        this Dependency<THttpMessageHandler> dependency,
        Func<IServiceProvider, CosmosStorageConfiguration> configurationResolver)
        where THttpMessageHandler : HttpMessageHandler
        =>
        InnerUseCosmosStorage(
            dependency ?? throw new ArgumentNullException(nameof(dependency)),
            configurationResolver ?? throw new ArgumentNullException(nameof(configurationResolver)));

    private static Dependency<IStorage> InnerUseCosmosStorage<THttpMessageHandler>(
        Dependency<THttpMessageHandler> dependency,
        Func<IServiceProvider, CosmosStorageConfiguration> configurationResolver)
        where THttpMessageHandler : HttpMessageHandler
        =>
        Dependency.From<HttpMessageHandler>(
            dependency.Resolve)
        .With(
            configurationResolver)
        .Fold<IStorage>(
            CosmosStorage.Create);
}