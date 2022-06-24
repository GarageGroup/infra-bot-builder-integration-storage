using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;

namespace GGroupp.Infra.Bot.Builder;

internal readonly record struct StorageHttpFailure
{
    private readonly IEnumerable<KeyValuePair<string, IEnumerable<string>>>? headers;

    private readonly string? failureMessage;

    public StorageHttpFailure(
        [AllowNull] IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
        HttpStatusCode failureCode,
        [AllowNull] string failureMessage)
    {
        this.headers = headers?.Any() is false ? null : headers;
        FailureCode = failureCode;
        this.failureMessage = string.IsNullOrEmpty(failureMessage) ? null : failureMessage;
    }

    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers
        =>
        headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>();

    public HttpStatusCode FailureCode { get; }

    public string FailureMessage
        =>
        failureMessage ?? string.Empty;
}