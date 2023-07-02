using System.Text;

namespace GarageGroup.Infra.Bot.Builder;

partial class ItemPathExtensions
{
    internal static string GetContainerId(this StorageItemLockPath path)
    {
        var builder = new StringBuilder("lock-state").Append("-state");

        if (string.IsNullOrEmpty(path.ChannelId))
        {
            return builder.ToString();
        }

        return builder.Append('-').Append(path.ChannelId).ToString();
    }
}