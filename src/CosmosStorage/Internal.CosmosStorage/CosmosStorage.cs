using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed partial class CosmosStorage : ICosmosStorage
{
    private const string ContainerResourceType = "colls";

    private const string ItemResourceType = "docs";

    private static readonly StoragePartitionKeyJson ContainerPartitionKey;

    private static readonly JsonSerializer JsonSerializer;

    static CosmosStorage()
    {
        ContainerPartitionKey = new(paths: new[] { "/id" }, kind: "Hash", version: 2);
        JsonSerializer = JsonSerializer.Create(new()
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore
        });
    }

    private readonly HttpMessageHandler httpMessageHandler;

    private readonly CosmosStorageOption option;

    private readonly HMACSHA256 hashAlgorithm;

    private readonly SemaphoreSlim semaphore;

    private bool disposed;

    internal CosmosStorage(HttpMessageHandler httpMessageHandler, CosmosStorageOption option)
    {
        this.httpMessageHandler = httpMessageHandler;
        this.option = option;

        hashAlgorithm = new()
        {
            Key = Convert.FromBase64String(option.MasterKey)
        };

        semaphore = new(option.MaxDegreeOfParallelism ?? 2);
    }

    private HttpClient CreateHttpClient(string escapedKey)
    {
        var client = CreateHttpClient();

        client.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", "[\"" + escapedKey + "\"]");
        return client;
    }

    private HttpClient CreateHttpClient()
        =>
        new(httpMessageHandler, false)
        {
            BaseAddress = option.BaseAddress
        };

    private static bool IsContainerExisted(HttpResponseMessage response)
    {
        return response.Headers.TryGetValues("x-ms-documentdb-partitionkeyrangeid", out var values) && values.Any(IsNotEmpty);

        static bool IsNotEmpty(string source)
            =>
            string.IsNullOrEmpty(source) is false;
    }

    private async Task CreateContainerAsync(string containerId, CosmosStorageContainerType type, CancellationToken cancellationToken)
    {
        var resourceId = $"dbs/{option.DatabaseId}";
        var ttlSeconds = option.ContainerTtlSeconds.TryGetValue(type, out var ttl) ? ttl : null;

        using var client = CreateHttpClient();

        var response = await client.GetResponseAsync(
            method: HttpMethod.Post,
            hashAlgorithm: hashAlgorithm,
            resourceId: resourceId,
            resourceType: ContainerResourceType,
            requestUri: resourceId + "/colls",
            contentJson: new StorageContainerJsonWrite(
                id: containerId,
                defaultTtlSeconds: ttlSeconds,
                partitionKey: ContainerPartitionKey),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (response.IsSuccessStatusCode is false && response.StatusCode is not HttpStatusCode.Conflict)
        {
            throw await GetUnexpectedStatusCodeExceptonAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<InvalidOperationException> GetUnexpectedStatusCodeExceptonAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var failureBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var failureMessageBuilder = new StringBuilder("Response code is unexpected:").Append(' ').Append(response.StatusCode);
        if (string.IsNullOrEmpty(failureBody) is false)
        {
            failureMessageBuilder = failureMessageBuilder.Append(".\n\r").Append(failureBody);
        }

        return new(failureMessageBuilder.ToString());
    }

    private static ObjectDisposedException CreateDisposedException()
        =>
        new("CosmosStorage has already been disposed");
}