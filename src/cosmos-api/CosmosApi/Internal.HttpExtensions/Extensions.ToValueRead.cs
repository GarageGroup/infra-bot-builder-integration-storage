using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class HttpExtensions
{
    internal static StorageValueRead ToValueRead(this JObject jObject)
        =>
        new(jsonSerializer, jObject);
}