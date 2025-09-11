using CQRS;
using Domain.Diary.DiaryRoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infras.Services.Queries
{
    public record GetDiaryByDailyIdRequest(Guid Id) : IRequest<DailyDiary>;
}
