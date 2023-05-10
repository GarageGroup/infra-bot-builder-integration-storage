namespace GarageGroup.Infra.Bot.Builder;

public interface IStorageValueRead
{
    T? GetProperty<T>(string property);

    object? ToObject();
}