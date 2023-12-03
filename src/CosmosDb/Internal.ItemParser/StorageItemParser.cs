using System.Text.RegularExpressions;

namespace GarageGroup.Infra.Bot.Builder;

internal static partial class StorageItemParser
{
    private static readonly Regex ItemPathRegex;

    static StorageItemParser()
        =>
        ItemPathRegex = CreateItemPathRegex();

    [GeneratedRegex("^([^/\\?#*]+)/(users|conversations)/([^/\\?#*]+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CreateItemPathRegex();
}