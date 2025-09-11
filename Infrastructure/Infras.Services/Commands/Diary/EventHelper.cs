using Domain.Diary.DiaryRoot;
using System.Text.Json;

namespace Infras.Services.Commands.Diary
{
    public static class EventHelper
    {
        public static object? GetDataFromBytes(this byte[] data, string type)
        {
            return (type) switch
            {
                nameof(AddSection) => JsonSerializer.Deserialize<AddSection>(data),
                nameof(RemoveSection) => JsonSerializer.Deserialize<RemoveSection>(data),
                nameof(PinSection) => JsonSerializer.Deserialize<PinSection>(data),
                _ => new { },
            };
        }
    }
}
