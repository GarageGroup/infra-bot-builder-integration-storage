using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GarageGroup.Infra.Bot.Builder;

internal static partial class ItemPathExtensions
{
    private const int MaxKeyLength = 255;

    private static readonly char[] EscapedSymbols;

    private static readonly IReadOnlyDictionary<char, string> Repleacements;

    static ItemPathExtensions()
    {
        EscapedSymbols = ['\\', '?', '/', '#', '*'];
        Repleacements = EscapedSymbols.ToDictionary(Pipeline.Pipe, GetReplace);

        static string GetReplace(char symbol)
            =>
            '*' + ((int)symbol).ToString("x2", CultureInfo.InvariantCulture);
    }
}