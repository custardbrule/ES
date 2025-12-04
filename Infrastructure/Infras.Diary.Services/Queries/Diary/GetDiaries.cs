using CQRS;
using Data;
using Domain.Diary.DiaryRoot;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Utilities;

namespace Infras.Diary.Services.Queries.Diary
{
    public record GetDiariesRequest(string? Name, string? AuthorId, int Page = 1, int PageSize = 10) : IRequest<EPagedList<DiaryViewModel>>;

    public class DiaryPageQuery : PageQuery<Domain.Diary.DiaryRoot.Diary>
    {
        public string? Name { get; set; }
        public string? AuthorId { get; set; }

        public override Query GetQuery()
        {
            var queries = new List<Query>();

            if (!string.IsNullOrWhiteSpace(Name))
                queries.Add(new MatchQuery { Field = new Field(nameof(Domain.Diary.DiaryRoot.Diary.Name)), Query = Name! });

            if (!string.IsNullOrWhiteSpace(AuthorId))
                queries.Add(new MatchQuery { Field = new Field(nameof(Domain.Diary.DiaryRoot.Diary.AuthorId)), Query = AuthorId! });

            return queries.Count switch
            {
                0 => new MatchAllQuery(),
                1 => queries[0], // no performance loss compare to default _, just more readable and no wrapper
                _ => new BoolQuery { Must = queries }
            };
        }

        public override SortOptions[] GetSort() => [
            new SortOptions() { Field = new FieldSort { Field = nameof(Domain.Diary.DiaryRoot.Diary.CreatedDate), Order = SortOrder.Desc } }
        ];

        public override string[] GetSourceIncludes() => [
            nameof(Domain.Diary.DiaryRoot.Diary.Id),
            nameof(Domain.Diary.DiaryRoot.Diary.Name),
            nameof(Domain.Diary.DiaryRoot.Diary.Description),
            nameof(Domain.Diary.DiaryRoot.Diary.AuthorId),
            nameof(Domain.Diary.DiaryRoot.Diary.Visibility),
            nameof(Domain.Diary.DiaryRoot.Diary.CreatedDate)
        ];
    }

    public class GetDiariesHandler : IHandler<GetDiariesRequest, EPagedList<DiaryViewModel>>
    {
        private readonly IElasticSearchContext _elasticsearchContext;

        public GetDiariesHandler(IElasticSearchContext elasticsearchContext)
        {
            _elasticsearchContext = elasticsearchContext;
        }

        public async Task<EPagedList<DiaryViewModel>> Handle(GetDiariesRequest request, CancellationToken cancellationToken)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

            var query = new DiaryPageQuery
            {
                Page = page,
                PageSize = pageSize,
                Name = request.Name,
                AuthorId = request.AuthorId
            };

            var searchRequest = query.GetSearchRequest(DiaryConstants.ESIndex);
            var searchResponse = await _elasticsearchContext.Client.SearchAsync<Domain.Diary.DiaryRoot.Diary>(searchRequest, cancellationToken);

            if (!searchResponse.IsValidResponse)
                throw new ApplicationException("Failed to retrieve diaries from Elasticsearch.");

            var items = searchResponse.Documents.Select(d => new DiaryViewModel(d.Id, d.Name, d.Description, d.AuthorId, d.Visibility, d.CreatedDate, []));
            var totalCount = searchResponse.Total;

            return EPagedList<DiaryViewModel>.Create(items, totalCount, page, pageSize);
        }
    }
}
