using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GGroupp.Infra.Bot.Builder;

partial class CosmosKeyExtensions
{
    internal static (string ContainerId, CosmosStorageContainerType ContainerType, string ItemId) ParseKey(this string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return (BotStorageContainerId, CosmosStorageContainerType.BotStorage, string.Empty);
        }

        var match = Regex.Match(key, "^([^/\\?#*]+)/(users|conversations)/([^/\\?#*]+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        if (match.Success is false)
        {
            return (BotStorageContainerId, CosmosStorageContainerType.BotStorage, InnerEscape(key));
        }

        var channelId = match.Groups[1].Value;
        var containerType = ParseContainerType(match.Groups[2].Value);
        var containerName = containerType is CosmosStorageContainerType.UserState ? "user" : "conversation";
        var itemId = match.Groups[3].Value;

        return ($"{containerName}-state-{channelId}", containerType, InnerTruncate(itemId));

        static CosmosStorageContainerType ParseContainerType(string containerTypeText)
            =>
            containerTypeText[0] is 'u' ? CosmosStorageContainerType.UserState : CosmosStorageContainerType.ConversationState;
    }

    private static string InnerEscape(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        var firstEscapedSymbolIndex = key.IndexOfAny(escapedSymbols);
        if (firstEscapedSymbolIndex is -1)
        {
            return InnerTruncate(key);
        }

        var keyBuilder = new StringBuilder(key.Length + ((key.Length - firstEscapedSymbolIndex + 1) * 3));
        for (var i = 0; i < key.Length; i++)
        {
            var symbol = key[i];
            if ((i >= firstEscapedSymbolIndex) && repleacements.TryGetValue(symbol, out var repleacement))
            {
                keyBuilder.Append(repleacement);
                continue;
            }

            keyBuilder.Append(symbol);
        }

        return InnerTruncate(keyBuilder.ToString());
    }

    private static string InnerTruncate(string key)
    {
        if (key.Length > MaxKeyLength is false)
        {
            return key;
        }

        var hash = key.GetHashCode().ToString("x", CultureInfo.InvariantCulture);
        return string.Concat(key.AsSpan(0, MaxKeyLength - hash.Length), hash);
    }
}