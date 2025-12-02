using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;

namespace Utilities
{
    public abstract class PageQuery<T>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public virtual Query GetQuery() => new MatchAllQuery();
        public virtual SortOptions[]? GetSort() => null;
        public virtual string[]? GetSourceIncludes() => null;

        public SearchRequest<T> GetSearchRequest(Indices indices)
        {
            var request = new SearchRequest<T>(indices)
            {
                From = (Page - 1) * PageSize,
                Size = PageSize,
                TrackTotalHits = new TrackHits(true),
                Query = GetQuery()
            };

            var sort = GetSort();
            if (sort != null && sort.Length > 0)
                request.Sort = sort;

            var sourceIncludes = GetSourceIncludes();
            if (sourceIncludes != null && sourceIncludes.Length > 0)
                request.SourceIncludes = sourceIncludes;

            return request;
        }

    }
    public class PagedList<T> : List<T>
    {
        private PagedList(IEnumerable<T> currentPage, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalCount = count;
            AddRange(currentPage);
        }

        public readonly IReadOnlyList<int> AvailablePageSizes = [10, 25, 50, 100];
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool IsPreviousPageExists => CurrentPage > 1;
        public bool IsNextPageExists => CurrentPage < TotalPages;

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var count = await source.CountAsync(ct);
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }

    public class EPagedList<T> : List<T>
    {
        private EPagedList(IEnumerable<T> currentPage, long count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalCount = count;
            AddRange(currentPage);
        }

        public readonly IReadOnlyList<int> AvailablePageSizes = [10, 25, 50, 100];
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool IsPreviousPageExists => CurrentPage > 1;
        public bool IsNextPageExists => CurrentPage < TotalPages;

        public static EPagedList<T> Create(IEnumerable<T> items, long totalCount, int pageNumber, int pageSize) => new EPagedList<T>(items, (int)totalCount, pageNumber, pageSize);
    }

    public static class Extensions
    {
        public static async Task<EPagedList<T>> GetPageList<T>(this ElasticsearchClient client, PageQuery<T> query, Indices indices)
        {
            var res = await client.SearchAsync<T>(query.GetSearchRequest(indices));

            if (!res.IsValidResponse)
                throw new ApplicationException("Failed to retrieve diaries from Elasticsearch.");

            return EPagedList<T>.Create(res.Documents, res.Total, query.Page, query.PageSize);
        }
    }
}