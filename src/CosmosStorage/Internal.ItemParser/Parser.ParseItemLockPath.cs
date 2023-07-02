namespace GarageGroup.Infra.Bot.Builder;

partial class StorageItemParser
{
    internal static StorageItemLockPath ParseItemLockPath(string source)
    {
        var match = ItemPathRegex.Match(source);

        if (match.Success is false)
        {
            return new(string.Empty, source);
        }

        return new(
            channelId: match.Groups[1].Value,
            itemId: $"{match.Groups[2].Value}:{match.Groups[3].Value}");
    }
}