using Newtonsoft.Json.Linq;

namespace GarageGroup.Infra.Bot.Builder;

partial class HttpExtensions
{
    internal static StorageValueRead ToValueRead(this JObject jObject)
        =>
        new(jsonSerializer, jObject);
}