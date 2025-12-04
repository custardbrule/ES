using CQRS;
using Data;
using Domain.Diary;
using Domain.Diary.DiaryRoot;
using Elastic.Clients.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infras.Diary.Services.Queries.Diary
{
    public record GetDailyDiaryByIdRequest(Guid Id) : IRequest<DailyDiaryViewModel>;

    public class GetDailyDiaryByIdHandler : IHandler<GetDailyDiaryByIdRequest, DailyDiaryViewModel>
    {
        private readonly IElasticSearchContext _elasticsearchContext;

        public GetDailyDiaryByIdHandler(IElasticSearchContext elasticsearchContext)
        {
            _elasticsearchContext = elasticsearchContext;
        }

        public async Task<DailyDiaryViewModel> Handle(GetDailyDiaryByIdRequest request, CancellationToken cancellationToken)
        {
            var response = await _elasticsearchContext.Client.GetAsync<DailyDiaryViewModel>(DailyDiaryConstants.ESIndex, request.Id, des => des.SourceIncludes(f => f.CreatedDate, f => f.Sections), cancellationToken);

            if (!response.IsValidResponse) throw new ApplicationException();
            if (!response.Found) throw new BussinessException("not_found", 400, "Item not found.");

            return response.Source!;
        }
    }
}
