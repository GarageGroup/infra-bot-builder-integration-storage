namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosApi
{
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (lazyHmacSha256.IsValueCreated)
        {
            lazyHmacSha256.Value.Dispose();
        }

        disposed = true;
    }
}