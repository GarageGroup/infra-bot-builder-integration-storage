using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class HttpExtensions
{
    internal static JObject? ToValueWrite(this object? value)
        =>
        value is null ? null : JObject.FromObject(value, jsonSerializer);
}