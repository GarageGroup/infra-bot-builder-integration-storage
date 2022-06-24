using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal static partial class HttpExtensions
{
    private const string ContainerResourceType = "colls";

    private const string ItemResourceType = "docs";

    private const int MaxKeyLength = 255;

    private static readonly char[] escapedSymbols;

    private static readonly IReadOnlyDictionary<char, string> repleacements;

    private static readonly JsonSerializerSettings jsonSerializerSettings;

    private static readonly JsonSerializer jsonSerializer;

    static HttpExtensions()
    {
        escapedSymbols = new[] { '\\', '?', '/', '#', '*' };
        repleacements = escapedSymbols.ToDictionary(c => c, GetReplace);

        jsonSerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        jsonSerializer = JsonSerializer.Create(new()
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore
        });

        static string GetReplace(char symbol)
            =>
            '*' + ((int)symbol).ToString("x2", CultureInfo.InvariantCulture);
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler httpMessageHandler, Uri baseAddress)
        =>
        new(httpMessageHandler, false)
        {
            BaseAddress = baseAddress
        };

    private static HttpClient AddStorageHeaders(
        this HttpClient httpClient, HashAlgorithm algorithm, string verb, string resourceId, string resourceType)
    {
        var utcDate = DateTime.UtcNow.ToString("r");

        httpClient.DefaultRequestHeaders.Add("x-ms-date", utcDate);
        httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");

        var authHeader = algorithm.GenerateAuthorizationHeaderValue(
            verb: verb, resourceId: resourceId, resourceType: resourceType, utcDate: utcDate);

        httpClient.DefaultRequestHeaders.Add("authorization", authHeader);
        return httpClient;
    }

    private static string GenerateAuthorizationHeaderValue(
        this HashAlgorithm hashAlgorithm, string verb, string resourceId, string resourceType, string utcDate)
    {
        var payLoad = $"{verb.ToLowerInvariant()}\n{resourceType}\n{resourceId}\n{utcDate.ToLowerInvariant()}\n\n";

        var hashPayLoad = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(payLoad));
        var signature = Convert.ToBase64String(hashPayLoad);

        var masterKeyAuthorizationSignature = $"type=master&ver=1.0&sig={signature}";
        return HttpUtility.UrlEncode(masterKeyAuthorizationSignature);
    }

    private static StringContent? CreateContent(object? content)
    {
        if (content is null)
        {
            return null;
        }

        var json = JsonConvert.SerializeObject(content, Formatting.Indented, jsonSerializerSettings);
        return new(json, Encoding.UTF8, MediaTypeNames.Application.Json);
    }

    private static string EscapeKey([AllowNull] string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        var firstEscapedSymbolIndex = key.IndexOfAny(escapedSymbols);
        if (firstEscapedSymbolIndex is -1)
        {
            return InnerTruncate(key);
        }

        var keyBuilder = new StringBuilder(key.Length + ((key.Length - firstEscapedSymbolIndex + 1) * 3));
        for (var i = 0; i < key.Length; i++)
        {
            var symbol = key[i];
            if ((i >= firstEscapedSymbolIndex) && repleacements.TryGetValue(symbol, out var repleacement))
            {
                keyBuilder.Append(repleacement);
                continue;
            }

            keyBuilder.Append(symbol);
        }

        return InnerTruncate(keyBuilder.ToString());
    }

    private static string InnerTruncate(string key)
    {
        if (key.Length > MaxKeyLength is false)
        {
            return key;
        }

        var hash = key.GetHashCode().ToString("x", CultureInfo.InvariantCulture);
        return string.Concat(key.AsSpan(0, MaxKeyLength - hash.Length), hash);
    }

    private static string CreateUnexpectedStatusCodeFailureMessage(HttpStatusCode statusCode, string? failureBody)
    {   
        var failureMessageBuilder = new StringBuilder($"Response code is unexpected: {statusCode}");
        if (string.IsNullOrEmpty(failureBody) is false)
        {
            return failureMessageBuilder.ToString();
        }

        var failureJson = JsonConvert.DeserializeObject<StorageFailureJson>(failureBody);
        if (string.IsNullOrEmpty(failureJson.Message))
        {
            return failureMessageBuilder.ToString();
        }

        return failureMessageBuilder.Append(". ").Append(failureJson.Message).ToString();
    }
}