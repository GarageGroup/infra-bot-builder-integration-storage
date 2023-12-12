using System.Text;

namespace GarageGroup.Infra.Bot.Builder;

partial class ItemPathExtensions
{
    internal static string GetContainerId(this IStorageContainerPath path)
    {
        var containerName = path.ItemType switch
        {
            StorageItemType.UserState => "user",
            StorageItemType.ConversationState => "conversation",
            _ => "bot"
        };

        var idBuilder = new StringBuilder(containerName).Append("-state");

        if (string.IsNullOrEmpty(path.ChannelId))
        {
            return idBuilder.ToString();
        }

        return idBuilder.Append('-').Append(path.ChannelId).ToString();
    }
}