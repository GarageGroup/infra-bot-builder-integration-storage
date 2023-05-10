using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public readonly record struct StorageItemLockFailure
{
    private readonly string? failureMessage;

    public StorageItemLockFailure([AllowNull] string failureMessage)
        =>
        this.failureMessage = string.IsNullOrEmpty(failureMessage) ? null : failureMessage;

    public string FailureMessage => failureMessage ?? string.Empty;
}