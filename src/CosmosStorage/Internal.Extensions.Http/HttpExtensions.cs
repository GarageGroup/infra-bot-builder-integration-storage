using System;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal static partial class HttpExtensions
{
    private const int MaxRetries = 300;

    private const int MinDelayMilliseconds = 900;

    private const int MaxDelayMilliseconds = 1000;

    private static readonly Random Random;

    private static readonly JsonSerializerSettings JsonSerializerSettings;

    static HttpExtensions()
    {
        Random = new();
        JsonSerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    private static HttpRequestMessage Authorize(
        this HttpRequestMessage requestMessage, HashAlgorithm hashAlgorithm, string verb, string resourceId, string resourceType)
    {
        var utcDate = DateTime.UtcNow.ToString("R");

        requestMessage.Headers.Add("x-ms-date", utcDate);
        requestMessage.Headers.Add("x-ms-version", "2018-12-31");

        var payLoad = $"{verb.ToLowerInvariant()}\n{resourceType}\n{resourceId}\n{utcDate.ToLowerInvariant()}\n\n";

        var hashPayLoad = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(payLoad));
        var signature = Convert.ToBase64String(hashPayLoad);

        var masterKeyAuthorizationSignature = $"type=master&ver=1.0&sig={signature}";
        var authorizationHeaderValue = HttpUtility.UrlEncode(masterKeyAuthorizationSignature);

        requestMessage.Headers.Add("authorization", authorizationHeaderValue);
        return requestMessage;
    }

    private static StringContent? CreateJsonContent<TJson>(this TJson? contentJson)
        where TJson : class
    {
        if (contentJson is null)
        {
            return null;
        }

        var body = JsonConvert.SerializeObject(contentJson, Formatting.Indented, JsonSerializerSettings);
        return new(body, Encoding.UTF8, MediaTypeNames.Application.Json);
    }

    private static Task Delay(CancellationToken cancellationToken)
    {
        var delayMilliseconds = Random.Next(MinDelayMilliseconds, MaxDelayMilliseconds);
        return Task.Delay(delayMilliseconds, cancellationToken);
    }
}