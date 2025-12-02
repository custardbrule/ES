using CQRS;
using Data;
using Domain.Diary.DiaryRoot;

namespace Infras.Services.Queries.Diary
{
    public record GetDiaryByIdRequest(Guid Id) : IRequest<DiaryViewModel>;

    public class GetDiaryByIdHandler : IHandler<GetDiaryByIdRequest, DiaryViewModel>
    {
        private readonly IElasticSearchContext _elasticsearchContext;

        public GetDiaryByIdHandler(IElasticSearchContext elasticsearchContext)
        {
            _elasticsearchContext = elasticsearchContext;
        }

        public async Task<DiaryViewModel> Handle(GetDiaryByIdRequest request, CancellationToken cancellationToken)
        {
            var taskGetDiary = _elasticsearchContext.Client.GetAsync<DiaryViewModel>(DiaryConstants.ESIndex, request.Id, cancellationToken);
            var taskGetDays = _elasticsearchContext.Client.SearchAsync<DailyDiaryViewModel>(r => r
                .Indices(DailyDiaryConstants.ESIndex)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m.Term(t => t.Field(f => f.DiaryId).Value(request.Id.ToString("N"))))
                    )
                )
                .TrackTotalHits(false),
                cancellationToken
            );

            await Task.WhenAll(taskGetDiary, taskGetDays);

            var diaryRes = await taskGetDiary;
            var days = await taskGetDays;

            if (!diaryRes.IsValidResponse || !days.IsValidResponse) throw new ApplicationException();
            if (!diaryRes.Found) throw new KeyNotFoundException();

            return diaryRes.Source! with { DailyDiaries = [.. days.Documents] };
        }
    }
}
