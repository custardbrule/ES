using Domain.Diary.DiaryRoot;
using System.Text.Json;

namespace Infras.Services
{
    public static class EventHelper
    {
        public static byte[] ObjectToBytes(this object obj) => JsonSerializer.SerializeToUtf8Bytes(obj);
        public static object? GetDataFromBytes(this byte[] data, string type)
        {
            return type switch
            {
                nameof(AddSection) => JsonSerializer.Deserialize<AddSection>(data),
                nameof(RemoveSection) => JsonSerializer.Deserialize<RemoveSection>(data),
                nameof(PinSection) => JsonSerializer.Deserialize<PinSection>(data),
                _ => new { },
            };
        }
    }
}
