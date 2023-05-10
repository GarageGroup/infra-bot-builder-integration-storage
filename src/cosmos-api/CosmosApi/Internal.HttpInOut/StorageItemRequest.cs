using System;
using System.Net.Http;
using System.Security.Cryptography;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class StorageItemRequest
{
    internal StorageItemRequest(
        HashAlgorithm hashAlgorithm,
        HttpMethod method,
        Uri baseAddress,
        string databaseId,
        string containerId,
        string itemId,
        object? content = null)
    {
        HashAlgorithm = hashAlgorithm;
        Method = method;
        BaseAddress = baseAddress;
        DatabaseId = databaseId ?? string.Empty;
        ContainerId = containerId ?? string.Empty;
        ItemId = itemId ?? string.Empty;
        Content = content;
    }

    public HashAlgorithm HashAlgorithm { get; }

    public HttpMethod Method { get; init; }

    public Uri BaseAddress { get; }

    public string DatabaseId { get; }

    public string ContainerId { get; }
    
    public string ItemId { get; }

    public object? Content { get; }
}