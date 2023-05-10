using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed partial class CosmosApi : ICosmosApi
{
    private static readonly StoragePartitionKeyJson partitionKey;

    static CosmosApi()
        =>
        partitionKey = new(paths: new[] { "/id" }, kind: "Hash", version: 2);

    private readonly HttpMessageHandler messageHandler;

    private readonly Uri baseAddress;

    private readonly Lazy<HMACSHA256> lazyHmacSha256;

    private readonly string databaseId;

    private bool disposed;

    internal CosmosApi(HttpMessageHandler messageHandler, CosmosApiOption option)
    {
        this.messageHandler = messageHandler;
        baseAddress = option.BaseAddress;
        lazyHmacSha256 = new(CreateHmacSha256, LazyThreadSafetyMode.ExecutionAndPublication);
        databaseId = HttpUtility.UrlEncode(option.DatabaseId.ToLowerInvariant());

        HMACSHA256 CreateHmacSha256()
            =>
            new()
            {
                Key = Convert.FromBase64String(option.MasterKey)
            };
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException("CosmosApi has already been disposed");
        }
    }

    private static Result<StorageItemPath, Failure<Unit>> ValidatePath(StorageItemPath? path)
        =>
        string.IsNullOrEmpty(path?.UserId) ? Failure.Create("UserId must be specified") : path;

    private static bool IsContainerExisted(StorageHttpFailure httpFailure)
    {
        return httpFailure.Headers?.FirstOrDefault(IsPartitionKeyRangeId).Value?.Any(IsNotEmpty) is true;

        static bool IsPartitionKeyRangeId(KeyValuePair<string, IEnumerable<string>> header)
            =>
            string.Equals(header.Key, "x-ms-documentdb-partitionkeyrangeid", StringComparison.InvariantCultureIgnoreCase);

        static bool IsNotEmpty(string? value)
            =>
            string.IsNullOrEmpty(value) is false;
    }

    private static string CreateContainerId(StorageItemPath path)
    {
        var containerName = path.ItemType switch
        {
            StorageItemType.UserState => "user",
            StorageItemType.ConversationState => "conversation",
            _ => "bot"
        };

        var idBuilder = new StringBuilder(containerName).Append("-state");

        if (string.IsNullOrEmpty(path.ChannelId))
        {
            return idBuilder.ToString();
        }

        return idBuilder.Append('-').Append(path.ChannelId).ToString();
    }

    private static string CreateContainerId(StorageItemLockPath input)
    {
        var idBuilder = new StringBuilder("lock-state").Append("-state");

        if (string.IsNullOrEmpty(input.ChannelId))
        {
            return idBuilder.ToString();
        }

        return idBuilder.Append('-').Append(input.ChannelId).ToString();
    }
}