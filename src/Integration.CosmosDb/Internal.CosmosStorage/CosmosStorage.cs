using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

    private const string ContinuationTokenHeaderName = "x-ms-continuation";

    private static readonly JsonSerializerSettings jsonSerializerSettings;

    private static readonly JsonSerializer jsonSerializer;

    static CosmosStorage()
    {
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

    private readonly string databaseId, userStateContainerId, defaultContainerId;

    private CosmosStorage(HttpMessageHandler httpMessageHandler, CosmosStorageConfiguration configuration)
    {
        this.httpMessageHandler = httpMessageHandler;
        baseAddress = configuration.BaseAddress;
        lazyHmacSha256 = new(CreateHmacSha256, LazyThreadSafetyMode.ExecutionAndPublication);
        databaseId = configuration.DatabaseId.ToLowerInvariant();
        defaultContainerId = configuration.DefaultContainerId.ToLowerInvariant();
        userStateContainerId = configuration.UserStateContainerId.ToLowerInvariant();

        HMACSHA256 CreateHmacSha256()
            =>
            new()
            {
                Key = Convert.FromBase64String(configuration.MasterKey)
            };
    }

    private HttpClient CreateHttpClient(string verb, string resourceId, string escapedKey)
    {
        var client = CreateBaseHttpClient(verb: verb, resourceId: resourceId);
        client.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", "[\"" + escapedKey + "\"]");

        return client;
    }

    private HttpClient CreateQueryHttpClient(string verb, string resourceId, string? continuationToken)
    {
        var client = CreateBaseHttpClient(verb: verb, resourceId: resourceId);

        client.DefaultRequestHeaders.Add("x-ms-documentdb-isquery", "true");
        client.DefaultRequestHeaders.Add("x-ms-documentdb-query-enablecrosspartition", "true");

        client.DefaultRequestHeaders.Add(ContinuationTokenHeaderName, continuationToken);

        return client;
    }

    private HttpClient CreateBaseHttpClient(string verb, string resourceId)
    {
        var client = new HttpClient(httpMessageHandler, false)
        {
            BaseAddress = baseAddress
        };

        var utcDate = DateTime.UtcNow.ToString("r");

        client.DefaultRequestHeaders.Add("x-ms-date", utcDate);
        client.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");

        var authHeader = GenerateMasterKeyAuthorizationSignature(verb, resourceId, utcDate);
        client.DefaultRequestHeaders.Add("authorization", authHeader);

        return client;
    }

    private string GenerateMasterKeyAuthorizationSignature(string verb, string resourceId, string utcDate)
    {
        var payLoad = Invariant($"{verb.ToLowerInvariant()}\ndocs\n{resourceId}\n{utcDate.ToLowerInvariant()}\n\n");

        var hashPayLoad = lazyHmacSha256.Value.ComputeHash(Encoding.UTF8.GetBytes(payLoad));
        var signature = Convert.ToBase64String(hashPayLoad);

        var masterKeyAuthorizationSignature = Invariant($"type=master&ver=1.0&sig={signature}");
        return HttpUtility.UrlEncode(masterKeyAuthorizationSignature);
    }

    private static string? GetContinuationToken(HttpResponseMessage response)
        =>
        response.Headers.TryGetValues(ContinuationTokenHeaderName, out var values) ? values?.FirstOrDefault() : default;

    private static StringContent CreateJsonContent<TJson>(TJson contentJson)
    {
        var body = JsonConvert.SerializeObject(contentJson, Formatting.Indented, jsonSerializerSettings);
        return new(body, Encoding.UTF8, MediaTypeNames.Application.Json);
    }

    private string GetContainerId(string key)
    {
        if (string.IsNullOrEmpty(userStateContainerId) || string.IsNullOrEmpty(key))
        {
            return defaultContainerId;
        }

        if (Regex.IsMatch(key, "^[^/]+/users/.*$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
        {
            return userStateContainerId;
        }

        return defaultContainerId;
    }

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