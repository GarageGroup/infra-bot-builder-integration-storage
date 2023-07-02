using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CosmosStorage
{
    public Task<StorageLockStatus> LockAsync(string key, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<StorageLockStatus>(cancellationToken);
        }

        if (disposed)
        {
            throw CreateDisposedException();
        }

        return semaphore.InvokeAsync(InnerLockAsync, key.OrEmpty(), cancellationToken);
    }

    private async Task<StorageLockStatus> InnerLockAsync(string key, CancellationToken cancellationToken)
    {
        var itemPath = StorageItemParser.ParseItemLockPath(key);

        var containerId = itemPath.GetContainerId();
        var itemId = itemPath.ItemId.EscapeItemId();

        var storageItem = new StorageItemJsonWrite(id: itemPath.ItemId, key: itemPath.ItemId, value: null);
        var resourceIdCreate = $"dbs/{option.DatabaseId}/colls/{containerId}";

        using var client = CreateHttpClient(escapedKey: itemId);

        var responseCreate = await client.GetResponseAsync(
            method: HttpMethod.Post,
            hashAlgorithm: hashAlgorithm,
            resourceId: resourceIdCreate,
            resourceType: ItemResourceType,
            requestUri: resourceIdCreate + "/docs",
            contentJson: storageItem,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (responseCreate.IsSuccessStatusCode)
        {
            return StorageLockStatus.Success;
        }

        if (responseCreate.StatusCode is HttpStatusCode.Conflict)
        {
            return StorageLockStatus.AlreadyLocked;
        }

        if (responseCreate.StatusCode is not HttpStatusCode.NotFound)
        {
            throw await GetUnexpectedStatusCodeExceptonAsync(responseCreate, cancellationToken).ConfigureAwait(false);
        }

        if (IsContainerExisted(responseCreate) is false)
        {
            await CreateContainerAsync(containerId, CosmosStorageContainerType.LockStorage, cancellationToken).ConfigureAwait(false);
        }

        var responseSecondCreate = await client.GetResponseAsync(
            method: HttpMethod.Post,
            hashAlgorithm: hashAlgorithm,
            resourceId: resourceIdCreate,
            resourceType: ItemResourceType,
            requestUri: resourceIdCreate + "/docs",
            contentJson: storageItem,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (responseSecondCreate.IsSuccessStatusCode)
        {
            return StorageLockStatus.Success;
        }

        if (responseSecondCreate.StatusCode is HttpStatusCode.Conflict)
        {
            return StorageLockStatus.AlreadyLocked;
        }

        throw await GetUnexpectedStatusCodeExceptonAsync(responseCreate, cancellationToken).ConfigureAwait(false);
    }
}