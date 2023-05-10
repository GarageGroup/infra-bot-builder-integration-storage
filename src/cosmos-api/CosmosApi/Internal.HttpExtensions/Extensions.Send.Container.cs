using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class HttpExtensions
{
    internal static async ValueTask<Result<Unit, StorageHttpFailure>> SendRequestAsync(
        this HttpMessageHandler httpMessageHandler, StorageContainerRequest request, CancellationToken cancellationToken)
    {
        var resourceId = $"dbs/{request.DatabaseId}";
        using var httpClient = InnerCreateHttpClient();

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, new StringBuilder(resourceId).Append("/colls").ToString())
        {
            Content = CreateContent(request.Content)
        };

        var response = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return default(Unit);
        }

        if (response.StatusCode is HttpStatusCode.Conflict)
        {
            return CreateHttpFailure($"A container {request.Content.Id} has already existed in the database {request.DatabaseId}");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return CreateHttpFailure(CreateUnexpectedStatusCodeFailureMessage(response.StatusCode, body));

        HttpClient InnerCreateHttpClient()
            =>
            CreateHttpClient(
                httpMessageHandler,
                request.BaseAddress)
            .AddStorageHeaders(
                algorithm: request.HashAlgorithm,
                verb: HttpMethod.Post.Method,
                resourceId: resourceId,
                resourceType: ContainerResourceType);

        StorageHttpFailure CreateHttpFailure(string failureMessage)
            =>
            new(
                headers: response.Headers,
                failureCode: response.StatusCode,
                failureMessage: failureMessage);
    }
}