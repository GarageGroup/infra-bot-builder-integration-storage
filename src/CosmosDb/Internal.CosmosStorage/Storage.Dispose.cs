namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        hashAlgorithm?.Dispose();
        semaphore?.Dispose();

        disposed = true;
    }
}