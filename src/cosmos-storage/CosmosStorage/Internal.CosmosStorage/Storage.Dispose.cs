namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (lazyCosmosApi.IsValueCreated)
        {
            lazyCosmosApi.Value.Dispose();
        }

        disposed = true;
    }
}