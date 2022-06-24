namespace GGroupp.Infra.Bot.Builder;

partial class CosmosStorage<TCosmosApi>
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