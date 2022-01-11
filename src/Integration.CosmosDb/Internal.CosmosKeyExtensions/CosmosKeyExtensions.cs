using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GGroupp.Infra.Bot.Builder;

internal static class CosmosKeyExtensions
{
    private const int MaxKeyLength = 255;

    private static readonly char[] escapedSymbols;

    private static readonly IReadOnlyDictionary<char, string> repleacements;

    static CosmosKeyExtensions()
    {
        escapedSymbols = new[] { '\\', '?', '/', '#', '*' };
        repleacements = escapedSymbols.ToDictionary(c => c, GetReplace);

        static string GetReplace(char symbol)
            =>
            '*' + ((int)symbol).ToString("x2", CultureInfo.InvariantCulture);
    }

    internal static string EscapeKey(this string key)
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