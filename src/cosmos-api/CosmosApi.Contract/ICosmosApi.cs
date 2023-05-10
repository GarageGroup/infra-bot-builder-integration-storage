namespace GarageGroup.Infra.Bot.Builder;

public interface ICosmosApi : IStorageItemReadSupplier, IStorageItemWriteSupplier, IStorageItemDeleteSupplier, IStorageItemLockSupplier
{
}