using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GGroupp.Infra.Bot.Builder;

internal static partial class CosmosKeyExtensions
{
    private const string BotStorageContainerId = "bot-storage";

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
}