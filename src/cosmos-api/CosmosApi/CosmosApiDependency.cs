using System;
using System.Net.Http;
using PrimeFuncPack;

namespace GGroupp.Infra.Bot.Builder;

using ICosmosApiProvider = IFunc<ICosmosApi>;

public static class CosmosApiDependency
{
    public static Dependency<ICosmosApiProvider> UseCosmosApi(
        this Dependency<HttpMessageHandler, CosmosApiOption> dependency)
    {
        _ = dependency ?? throw new ArgumentNullException(nameof(dependency));

        return dependency.Fold<ICosmosApiProvider>(CosmosApiProvider.Create);
    }

    public static Dependency<ICosmosApiProvider> UseCosmosApi(
        this Dependency<HttpMessageHandler> dependency, Func<IServiceProvider, CosmosApiOption> optionResolver)
    {
        _ = dependency ?? throw new ArgumentNullException(nameof(dependency));
        _ = optionResolver ?? throw new ArgumentNullException(nameof(optionResolver));

        return dependency.With(optionResolver).Fold<ICosmosApiProvider>(CosmosApiProvider.Create);
    }
}