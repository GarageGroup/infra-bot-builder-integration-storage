using System;
using System.Security.Cryptography;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class StorageContainerRequest
{
    internal StorageContainerRequest(
        HashAlgorithm hashAlgorithm,
        Uri baseAddress,
        string databaseId,
        StorageContainerJsonWrite content)
    {
        HashAlgorithm = hashAlgorithm;
        BaseAddress = baseAddress;
        DatabaseId = databaseId ?? string.Empty;
        Content = content;
    }

    public HashAlgorithm HashAlgorithm { get; }

    public Uri BaseAddress { get; }

    public string DatabaseId { get; }
    
    public StorageContainerJsonWrite Content { get; }
}