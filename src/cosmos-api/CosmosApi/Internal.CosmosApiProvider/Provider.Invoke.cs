namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosApiProvider
{
    public CosmosApi Invoke() => new(httpMessageHandler, option);
}