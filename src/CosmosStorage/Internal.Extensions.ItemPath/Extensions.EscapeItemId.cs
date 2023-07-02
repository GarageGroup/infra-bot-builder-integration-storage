using System;
using System.Globalization;
using System.Text;

namespace GarageGroup.Infra.Bot.Builder;

partial class ItemPathExtensions
{
    internal static string EscapeItemId(this string? itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return string.Empty;
        }

        var firstEscapedSymbolIndex = itemId.IndexOfAny(EscapedSymbols);
        if (firstEscapedSymbolIndex is -1)
        {
            return InnerTruncate(itemId);
        }

        var keyBuilder = new StringBuilder(itemId.Length + ((itemId.Length - firstEscapedSymbolIndex + 1) * 3));

        for (var i = 0; i < itemId.Length; i++)
        {
            var symbol = itemId[i];
            if ((i >= firstEscapedSymbolIndex) && Repleacements.TryGetValue(symbol, out var repleacement))
            {
                keyBuilder.Append(repleacement);
                continue;
            }

            keyBuilder.Append(symbol);
        }

        return InnerTruncate(keyBuilder.ToString());

        static string InnerTruncate(string key)
        {
            if (key.Length > MaxKeyLength is false)
            {
                return key;
            }

            var hash = key.GetHashCode().ToString("x", CultureInfo.InvariantCulture);
            return string.Concat(key.AsSpan(0, MaxKeyLength - hash.Length), hash);
        }
    }
}