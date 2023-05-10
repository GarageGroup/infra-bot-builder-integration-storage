using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public readonly record struct StorageItemDeleteFailure
{
    private readonly string? failureMessage;

    public StorageItemDeleteFailure(StorageItemDeleteFailureCode failureCode, [AllowNull] string failureMessage)
    {
        FailureCode = failureCode;
        this.failureMessage = string.IsNullOrEmpty(failureMessage) ? null : failureMessage;
    }

    public StorageItemDeleteFailureCode FailureCode { get; }

    public string FailureMessage => failureMessage ?? string.Empty;
}