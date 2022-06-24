using System;
using PrimeFuncPack;

namespace GGroupp.Infra.Bot.Builder;

public static class CosmosStorageDependency
{
    public static Dependency<ICosmosStorage> UseCosmosStorage<TCosmosApi>(
        this Dependency<IFunc<TCosmosApi>, CosmosStorageOption> dependency)
        where TCosmosApi : class, IStorageItemReadSupplier, IStorageItemWriteSupplier, IStorageItemDeleteSupplier
    {
        _ = dependency ?? throw new ArgumentNullException(nameof(dependency));

        return dependency.Fold<ICosmosStorage>(CosmosStorage<TCosmosApi>.Create);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorage<TCosmosApi>(
        this Dependency<IFunc<TCosmosApi>> dependency)
        where TCosmosApi : class, IStorageItemReadSupplier, IStorageItemWriteSupplier, IStorageItemDeleteSupplier
    {
        _ = dependency ?? throw new ArgumentNullException(nameof(dependency));

        return dependency.Map<ICosmosStorage>(CosmosStorage<TCosmosApi>.Create);
    }

    public static Dependency<ICosmosStorage> UseCosmosStorage<TCosmosApi>(
        this Dependency<IFunc<TCosmosApi>> dependency, Func<IServiceProvider, CosmosStorageOption> optionResolver)
        where TCosmosApi : class, IStorageItemReadSupplier, IStorageItemWriteSupplier, IStorageItemDeleteSupplier
    {
        _ = dependency ?? throw new ArgumentNullException(nameof(dependency));
        _ = optionResolver ?? throw new ArgumentNullException(nameof(optionResolver));

        return dependency.With(optionResolver).Fold<ICosmosStorage>(CosmosStorage<TCosmosApi>.Create);
    }
}