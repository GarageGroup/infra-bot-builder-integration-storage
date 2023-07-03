using System.Text.RegularExpressions;

namespace GarageGroup.Infra.Bot.Builder;

internal static partial class StorageItemParser
{
    private static readonly Regex ItemPathRegex;

    static StorageItemParser()
        =>
        ItemPathRegex = CreateItemPathRegex();

#if NET7_0_OR_GREATER
    [GeneratedRegex("^([^/\\?#*]+)/(users|conversations)/([^/\\?#*]+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CreateItemPathRegex();
#else
    private static Regex CreateItemPathRegex()
        =>
        new("^([^/\\?#*]+)/(users|conversations)/([^/\\?#*]+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
#endif
}