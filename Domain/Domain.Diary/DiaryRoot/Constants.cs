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
        public const string DailyDiaryIndex = "daily_diary";
        public const string DailyDiaryStreamPrefix = $"{Base}_";
        public static string GetStreamName(string id) => $"{DailyDiaryStreamPrefix}{id}";
        public static string GetStreamName(Guid id) => $"{DailyDiaryStreamPrefix}{id:N}";
    }
}
