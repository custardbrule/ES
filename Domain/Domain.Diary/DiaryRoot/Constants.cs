using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diary.DiaryRoot
{
    public class DiaryConstants
    {
        public const string Base = nameof(DailyDiary);
        public const string DailyDiaryIndex = "es_daily_diary";
        public static string GetId(Guid id) => $"{id:N}";
        public static string GetStreamName(string id, string timeZoneId) => $"{Base}_{id}_{GetDateFromTimeZone(timeZoneId)}";
        public static string GetStreamName(Guid id, string timeZoneId) => $"{Base}_{id:N}_{GetDateFromTimeZone(timeZoneId)}";

        private static string GetDateFromTimeZone(string timeZoneId) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)).ToString("ddMMyyyy");
    }
}
