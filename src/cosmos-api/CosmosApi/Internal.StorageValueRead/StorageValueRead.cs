using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

internal sealed class StorageValueRead : IStorageValueRead
{
    private readonly JsonSerializer jsonSerializer;

    private readonly JObject? jObject;

    internal StorageValueRead(JsonSerializer jsonSerializer, JObject? jObject)
    {
        this.jsonSerializer = jsonSerializer;
        this.jObject = jObject;
    }

    public T? GetProperty<T>(string property)
        =>
        jObject is not null && jObject.TryGetValue(property, out var jToken) ? jToken.ToObject<T>() : default;

    public object? ToObject()
        =>
        jObject?.ToObject<object>(jsonSerializer);
}