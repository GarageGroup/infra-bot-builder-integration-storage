using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeFuncPack;

namespace GarageGroup.Infra.Bot.Builder;

public static class CosmosStorageDependency
{
    public static Dependency<ICosmosStorage> UseCosmosStorage(this Dependency<HttpMessageHandler, CosmosStorageOption> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.Fold<ICosmosStorage>(CreateStorage);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorage(
        this Dependency<HttpMessageHandler> dependency, Func<IServiceProvider, CosmosStorageOption> optionResolver)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(optionResolver);

        return dependency.With(optionResolver).Fold<ICosmosStorage>(CreateStorage);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorage(this Dependency<HttpMessageHandler> dependency, string sectionName = "CosmosDb")
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.With(ResolveOption).Fold<ICosmosStorage>(CreateStorage);

        CosmosStorageOption ResolveOption(IServiceProvider serviceProvider)
            =>
            serviceProvider.GetRequiredService<IConfiguration>().GetRequiredSection(sectionName.OrEmpty()).ReadCosmosStorageOption();
    }

    private static CosmosStorage CreateStorage(HttpMessageHandler httpMessageHandler, CosmosStorageOption option)
    {
        ArgumentNullException.ThrowIfNull(httpMessageHandler);
        ArgumentNullException.ThrowIfNull(option);

        return new(httpMessageHandler, option);
    }

    private static CosmosStorageOption ReadCosmosStorageOption(this IConfigurationSection section)
        =>
        new(
            baseAddress: section.GetUriOrThrow("BaseAddressUrl"),
            masterKey: section["MasterKey"].OrEmpty(),
            databaseId: section["DatabaseId"].OrEmpty(),
            containerTtlSeconds: new Dictionary<CosmosStorageContainerType, int?>
            {
                [CosmosStorageContainerType.UserState] = section.GetTtlSeconds("UserStateContainerTtlHours"),
                [CosmosStorageContainerType.ConversationState] = section.GetTtlSeconds("ConversationStateContainerTtlHours"),
                [CosmosStorageContainerType.BotStorage] = section.GetTtlSeconds("BotStorageContainerTtlHours")
            });

    private static Uri GetUriOrThrow(this IConfigurationSection section, string key)
    {
        var value = section[key];

        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Configuration key '{key}' in the section '{section.Key}' must be specified");
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) is false)
        {
            throw new InvalidOperationException($"Configuration key '{key}' in the section '{section.Key}' must be a valid uri. Value '{value}'");
        }

        return uri;
    }

    private static int? GetTtlSeconds(this IConfiguration configuration, string ttlHoursKey)
    {
        var ttlHours = configuration.GetValue<int?>(ttlHoursKey);
        return ttlHours is not null ? ttlHours.Value * 3600 : null;
    }
}