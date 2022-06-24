using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public readonly record struct StorageItemWriteFailure
{
    private readonly string? failureMessage;

    public StorageItemWriteFailure(StorageItemWriteFailureCode failureCode, [AllowNull] string failureMessage)
    {
        FailureCode = failureCode;
        this.failureMessage = string.IsNullOrEmpty(failureMessage) ? null : failureMessage;
    }

    public StorageItemWriteFailureCode FailureCode { get; }

    public string FailureMessage => failureMessage ?? string.Empty;
}