using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

partial class HttpExtensions
{
    internal static async ValueTask<Result<TJson, StorageHttpFailure>> SendRequestAsync<TJson>(
        this HttpMessageHandler httpMessageHandler,
        StorageItemRequest request,
        bool isSuccessFromBody,
        CancellationToken cancellationToken)
        where TJson : struct
    {
        var escapedKey = EscapeKey(request.ItemId);
        var resourceId = $"dbs/{request.DatabaseId}/colls/{request.ContainerId}";

        if (request.Method != HttpMethod.Post)
        {
            resourceId = new StringBuilder(resourceId).Append("/docs/").Append(escapedKey).ToString();
        }

        using var httpClient = InnerCreateHttpClient();

        using var httpRequest = new HttpRequestMessage(
            method: request.Method,
            requestUri: request.Method != HttpMethod.Post ? resourceId : new StringBuilder(resourceId).Append("/docs").ToString())
        {
            Content = CreateContent(request.Content)
        };

        var response = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode && isSuccessFromBody is false)
        {
            return default(TJson);
        }

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return CreateHttpFailure($"Item {request.ItemId} in container {request.ContainerId} was not found");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode is false)
        {
            return CreateHttpFailure(CreateUnexpectedStatusCodeFailureMessage(response.StatusCode, body));
        }

        return string.IsNullOrEmpty(body) ? default : JsonConvert.DeserializeObject<TJson>(body);

        HttpClient InnerCreateHttpClient()
            =>
            CreateHttpClient(
                httpMessageHandler,
                request.BaseAddress)
            .AddStorageHeaders(
                algorithm: request.HashAlgorithm,
                verb: request.Method.Method,
                resourceId: resourceId,
                resourceType: ItemResourceType)
            .AddPartitionKeyHeader(
                escapedKey);

        StorageHttpFailure CreateHttpFailure(string failureMessage)
            =>
            new(
                headers: response.Headers,
                failureCode: response.StatusCode,
                failureMessage: failureMessage);
    }

    private static HttpClient AddPartitionKeyHeader(this HttpClient httpClient, string escapedKey)
    {
        httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", "[\"" + escapedKey + "\"]");
        return httpClient;
    }
}