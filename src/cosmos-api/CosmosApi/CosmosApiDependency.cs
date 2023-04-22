using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using PrimeFuncPack;

namespace GGroupp.Infra.Bot.Builder;

using ICosmosApiProvider = IFunc<ICosmosApi>;

public static class CosmosApiDependency
{
    public static Dependency<ICosmosApiProvider> UseCosmosApi(
        this Dependency<HttpMessageHandler, CosmosApiOption> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.Fold<ICosmosApiProvider>(CosmosApiProvider.Create);
    }

    public static Dependency<ICosmosApiProvider> UseCosmosApi(
        this Dependency<HttpMessageHandler> dependency, Func<IServiceProvider, CosmosApiOption> optionResolver)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(optionResolver);

        return dependency.With(optionResolver).Fold<ICosmosApiProvider>(CosmosApiProvider.Create);
    }

    public static Dependency<ICosmosApiProvider> UseCosmosApi(
        this Dependency<HttpMessageHandler> dependency, string sectionName = "CosmosDb")
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.With(ResolveOption).Fold<ICosmosApiProvider>(CosmosApiProvider.Create);

        CosmosApiOption ResolveOption(IServiceProvider serviceProvider)
            =>
            serviceProvider.GetServiceOrThrow<IConfiguration>().GetRequiredSection(sectionName ?? string.Empty).ReadCosmosApiOption();
    }

    private static CosmosApiOption ReadCosmosApiOption(this IConfigurationSection section)
        =>
        new(
            baseAddress: new(section["BaseAddressUrl"] ?? string.Empty),
            masterKey: section["MasterKey"] ?? string.Empty,
            databaseId: section["DatabaseId"] ?? string.Empty);
}