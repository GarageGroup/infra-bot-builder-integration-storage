using System;
using System.Net.Http;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed partial class CosmosApiProvider : IFunc<CosmosApi>
{
    public static CosmosApiProvider Create(HttpMessageHandler httpMessageHandler, CosmosApiOption option)
        =>
        new(
            httpMessageHandler ?? throw new ArgumentNullException(nameof(httpMessageHandler)),
            option ?? throw new ArgumentNullException(nameof(option)));

    private readonly HttpMessageHandler httpMessageHandler;

    private readonly CosmosApiOption option;

    private CosmosApiProvider(HttpMessageHandler httpMessageHandler, CosmosApiOption option)
    {
        this.httpMessageHandler = httpMessageHandler;
        this.option = option;
    }
}