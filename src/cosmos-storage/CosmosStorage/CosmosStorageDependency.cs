using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using PrimeFuncPack;

namespace GarageGroup.Infra.Bot.Builder;

public static class CosmosStorageDependency
{
    private const int LockStorageDefaultTtlSeconds = 300;

    public static Dependency<ICosmosStorage> UseCosmosStorage(this Dependency<IFunc<ICosmosApi>, CosmosStorageOption> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.Fold<ICosmosStorage>(CosmosStorage.Create);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorage(this Dependency<IFunc<ICosmosApi>> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.Map<ICosmosStorage>(CosmosStorage.Create);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorage(
        this Dependency<IFunc<ICosmosApi>> dependency, Func<IServiceProvider, CosmosStorageOption> optionResolver)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(optionResolver);

        return dependency.With(optionResolver).Fold<ICosmosStorage>(CosmosStorage.Create);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorageStandard(this Dependency<IFunc<ICosmosApi>> dependency, string sectionName)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.With(ResolveOption).Fold<ICosmosStorage>(CosmosStorage.Create);

        CosmosStorageOption ResolveOption(IServiceProvider serviceProvider)
            =>
            serviceProvider.GetServiceOrThrow<IConfiguration>().GetRequiredSection(sectionName ?? string.Empty).ReadCosmosStorageOption();
    }

    private static CosmosStorageOption ReadCosmosStorageOption(this IConfigurationSection section)
        =>
        new(
            containerTtlSeconds: new Dictionary<CosmosStorageContainerType, int?>
            {
                [CosmosStorageContainerType.UserState] = section.GetTtlSeconds("UserStateContainerTtlHours"),
                [CosmosStorageContainerType.ConversationState] = section.GetTtlSeconds("ConversationStateContainerTtlHours"),
                [CosmosStorageContainerType.BotStorage] = section.GetTtlSeconds("BotStorageContainerTtlHours"),
                [CosmosStorageContainerType.LockStorage] = section.GetTtlSeconds("LockStorageContainerTtlHours") ?? LockStorageDefaultTtlSeconds
            });

    private static int? GetTtlSeconds(this IConfiguration configuration, string ttlHoursKey)
    {
        var ttlHours = configuration.GetValue<int?>(ttlHoursKey);
        return ttlHours is not null ? ttlHours.Value * 3600 : null;
    }
}