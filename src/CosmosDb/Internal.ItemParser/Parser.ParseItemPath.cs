namespace GarageGroup.Infra.Bot.Builder;

partial class StorageItemParser
{
    internal static StorageItemPath ParseItemPath(string source)
    {
        var match = ItemPathRegex.Match(source);

        if (match.Success is false)
        {
            return new(
                itemType: default,
                channelId: string.Empty,
                itemId: source);
        }

        return new(
            itemType: match.Groups[2].Value[0] is 'u' ? StorageItemType.UserState : StorageItemType.ConversationState,
            channelId: match.Groups[1].Value,
            itemId: match.Groups[3].Value);
    }
}