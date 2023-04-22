using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using PrimeFuncPack;

namespace GGroupp.Infra.Bot.Builder;

public static class CosmosStorageDependency
{
    public static Dependency<ICosmosStorage> UseCosmosStorage<TCosmosApi>(
        this Dependency<IFunc<TCosmosApi>, CosmosStorageOption> dependency)
        where TCosmosApi : class, IStorageItemReadSupplier, IStorageItemWriteSupplier, IStorageItemDeleteSupplier
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.Fold<ICosmosStorage>(CosmosStorage<TCosmosApi>.Create);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorage<TCosmosApi>(
        this Dependency<IFunc<TCosmosApi>> dependency)
        where TCosmosApi : class, IStorageItemReadSupplier, IStorageItemWriteSupplier, IStorageItemDeleteSupplier
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.Map<ICosmosStorage>(CosmosStorage<TCosmosApi>.Create);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorage<TCosmosApi>(
        this Dependency<IFunc<TCosmosApi>> dependency, Func<IServiceProvider, CosmosStorageOption> optionResolver)
        where TCosmosApi : class, IStorageItemReadSupplier, IStorageItemWriteSupplier, IStorageItemDeleteSupplier
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(optionResolver);

        return dependency.With(optionResolver).Fold<ICosmosStorage>(CosmosStorage<TCosmosApi>.Create);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorageStandard<TCosmosApi>(
        this Dependency<IFunc<TCosmosApi>> dependency, string sectionName)
        where TCosmosApi : class, IStorageItemReadSupplier, IStorageItemWriteSupplier, IStorageItemDeleteSupplier
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.With(ResolveOption).Fold<ICosmosStorage>(CosmosStorage<TCosmosApi>.Create);

        CosmosStorageOption ResolveOption(IServiceProvider serviceProvider)
            =>
            serviceProvider.GetServiceOrThrow<IConfiguration>().GetRequiredSection(sectionName ?? string.Empty).ReadCosmosStorageOption();
    }

    private static CosmosStorageOption ReadCosmosStorageOption(this IConfigurationSection section)
        =>
        new(
            containerTtlSeconds: new Dictionary<StorageItemType, int?>
            {
                [StorageItemType.UserState] = section.GetTtlSeconds("UserStateContainerTtlHours"),
                [StorageItemType.ConversationState] = section.GetTtlSeconds("ConversationStateContainerTtlHours"),
                [StorageItemType.Default] = section.GetTtlSeconds("BotStorageContainerTtlHours")
            });

    private static int? GetTtlSeconds(this IConfiguration configuration, string ttlHoursKey)
    {
        var ttlHours = configuration.GetValue<int?>(ttlHoursKey);
        return ttlHours is not null ? ttlHours.Value * 3600 : null;
    }
}