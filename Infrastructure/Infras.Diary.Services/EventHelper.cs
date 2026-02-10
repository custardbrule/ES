using Domain.Diary.DiaryRoot;
using System.Text.Json;

namespace Infras.Diary.Services
{
    public static class EventHelper
    {
        public static byte[] ObjectToBytes(this object obj) => JsonSerializer.SerializeToUtf8Bytes(obj);
        public static object? GetDataFromBytes(this byte[] data, string type)
        {
            return type switch
            {
                // Diary events
                nameof(InitDiary) => JsonSerializer.Deserialize<InitDiary>(data),
                nameof(ChangeDiaryVisibility) => JsonSerializer.Deserialize<ChangeDiaryVisibility>(data),
                nameof(ChangeDiaryInfo) => JsonSerializer.Deserialize<ChangeDiaryInfo>(data),
                // DailyDiary events
                nameof(InitDailyDiary) => JsonSerializer.Deserialize<InitDailyDiary>(data),
                nameof(AddSection) => JsonSerializer.Deserialize<AddSection>(data),
                nameof(PinSection) => JsonSerializer.Deserialize<PinSection>(data),
                _ => new { },
            };
        }
    }
}
