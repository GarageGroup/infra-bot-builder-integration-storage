using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public readonly record struct StorageItemReadFailure
{
    private readonly string? failureMessage;

    public StorageItemReadFailure(StorageItemReadFailureCode failureCode, [AllowNull] string failureMessage)
    {
        FailureCode = failureCode;
        this.failureMessage = string.IsNullOrEmpty(failureMessage) ? null : failureMessage;
    }

    public StorageItemReadFailureCode FailureCode { get; }

    public string FailureMessage => failureMessage ?? string.Empty;
}