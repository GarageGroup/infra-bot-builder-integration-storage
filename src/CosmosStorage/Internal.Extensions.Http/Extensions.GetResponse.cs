using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class HttpExtensions
{
    internal static async Task<HttpResponseMessage> GetResponseAsync<TJson>(
        this HttpClient httpClient,
        HttpMethod method,
        HashAlgorithm hashAlgorithm,
        string resourceId,
        string resourceType,
        string requestUri,
        TJson? contentJson,
        CancellationToken cancellationToken)
        where TJson : class
    {
        var retries = 0;

        while(true)
        {
            using var request = new HttpRequestMessage(method, requestUri)
            {
                Content = CreateJsonContent(contentJson)
            }
            .Authorize(hashAlgorithm, method.Method, resourceId, resourceType);

            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (retries++ >= MaxRetries || response.IsSuccessStatusCode)
            {
                return response;
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized)
            {
                await Delay(cancellationToken).ConfigureAwait(false);
                continue;
            }

            return response;
        }
    }
}