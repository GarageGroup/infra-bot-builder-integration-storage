using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using static System.FormattableString;

namespace GGroupp.Infra.Bot.Builder;

internal sealed partial class CosmosStorage : IStorage
{
    public static CosmosStorage Create(HttpMessageHandler httpMessageHandler, CosmosStorageConfiguration configuration)
        =>
        new(
            httpMessageHandler ?? throw new ArgumentNullException(nameof(httpMessageHandler)),
            configuration ?? throw new ArgumentNullException(nameof(configuration)));

    private const string ContainerResourceType = "colls";

    private const string ItemResourceType = "docs";

    private static readonly StoragePartitionKeyJson partitionKey;

    private static readonly JsonSerializerSettings jsonSerializerSettings;

    private static readonly JsonSerializer jsonSerializer;

    static CosmosStorage()
    {
        partitionKey = new(
            paths: new[] { "/id" },
            kind: "Hash",
            version: 2);
        jsonSerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        jsonSerializer = JsonSerializer.Create(new()
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore
        });
    }

    private readonly HttpMessageHandler httpMessageHandler;

    private readonly Uri baseAddress;

    private readonly Lazy<HMACSHA256> lazyHmacSha256;

    private readonly string databaseId;

    private readonly IReadOnlyDictionary<CosmosStorageContainerType, int?> containerTtlSeconds;

    private CosmosStorage(HttpMessageHandler httpMessageHandler, CosmosStorageConfiguration configuration)
    {
        this.httpMessageHandler = httpMessageHandler;
        baseAddress = configuration.BaseAddress;
        lazyHmacSha256 = new(CreateHmacSha256, LazyThreadSafetyMode.ExecutionAndPublication);
        databaseId = configuration.DatabaseId.ToLowerInvariant();
        containerTtlSeconds = configuration.ContainerTtlSeconds;

        HMACSHA256 CreateHmacSha256()
            =>
            new()
            {
                Key = Convert.FromBase64String(configuration.MasterKey)
            };
    }

    private HttpClient CreateHttpClient(string verb, string resourceId, string resourceType, string escapedKey)
    {
        var client = CreateHttpClient(verb: verb, resourceId: resourceId, resourceType: resourceType);
        client.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", "[\"" + escapedKey + "\"]");

        return client;
    }

    private HttpClient CreateHttpClient(string verb, string resourceId, string resourceType)
    {
        var client = new HttpClient(httpMessageHandler, false)
        {
            BaseAddress = baseAddress
        };

        var utcDate = DateTime.UtcNow.ToString("r");

        client.DefaultRequestHeaders.Add("x-ms-date", utcDate);
        client.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");

        var authHeader = GenerateAuthorizationHeaderValue(verb: verb, resourceId: resourceId, resourceType: resourceType, utcDate: utcDate);
        client.DefaultRequestHeaders.Add("authorization", authHeader);

        return client;
    }

    private string GenerateAuthorizationHeaderValue(string verb, string resourceId, string resourceType, string utcDate)
    {
        var payLoad = Invariant($"{verb.ToLowerInvariant()}\n{resourceType}\n{resourceId}\n{utcDate.ToLowerInvariant()}\n\n");

        var hashPayLoad = lazyHmacSha256.Value.ComputeHash(Encoding.UTF8.GetBytes(payLoad));
        var signature = Convert.ToBase64String(hashPayLoad);

        var masterKeyAuthorizationSignature = Invariant($"type=master&ver=1.0&sig={signature}");
        return HttpUtility.UrlEncode(masterKeyAuthorizationSignature);
    }

    private static StringContent CreateJsonContent<TJson>(TJson contentJson)
    {
        var body = JsonConvert.SerializeObject(contentJson, Formatting.Indented, jsonSerializerSettings);
        return new(body, Encoding.UTF8, MediaTypeNames.Application.Json);
    }

    private int? GetTtlSeconds(CosmosStorageContainerType containerType)
        =>
        containerTtlSeconds.TryGetValue(containerType, out var ttl) ? ttl : null;

    private static async Task<InvalidOperationException> CreateUnexpectedStatusCodeExceptonAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var failureBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        
        var failureMessageBuilder = new StringBuilder(Invariant($"Response code is unexpected: {response.StatusCode}"));
        if (string.IsNullOrEmpty(failureBody) is false)
        {
            failureMessageBuilder = failureMessageBuilder.Append(".\n\r").Append(failureBody);
        }

        return new(failureMessageBuilder.ToString());
    }
}