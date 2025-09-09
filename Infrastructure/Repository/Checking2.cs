using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;

// Custom fluent query builder
public class ElasticQueryBuilder<T> where T : class
{
    private readonly ElasticsearchClient _client;
    private readonly List<Action<SearchRequestDescriptor<T>>> _configurations = new();
    private string _indexName;

    public ElasticQueryBuilder(ElasticsearchClient client, string indexName = null)
    {
        _client = client;
        _indexName = indexName;
    }

    public ElasticQueryBuilder<T> Index(string indexName)
    {
        _indexName = indexName;
        return this;
    }

    public ElasticQueryBuilder<T> Match(string field, string value, double? boost = null)
    {
        _configurations.Add(s => s.Query(q => q
            .Match(m => m
                .Field(field)
                .Query(value)
                .Boost(boost)
            )
        ));
        return this;
    }

    public ElasticQueryBuilder<T> Term(string field, object value)
    {
        _configurations.Add(s => s.Query(q => q
            .Term(t => t
                .Field(field)
                .Value(FieldValue.String(value.ToString()))
            )
        ));
        return this;
    }

    public ElasticQueryBuilder<T> Range(string field, object from = null, object to = null)
    {
        _configurations.Add(s => s.Query(q => q
            .Range(r => r
                .NumberRange(nr =>
                {
                    var range = nr.Field(field);
                    if (from != null) range = range.Gte(Convert.ToDouble(from));
                    if (to != null) range = range.Lte(Convert.ToDouble(to));
                    return range;
                })
            )
        ));
        return this;
    }

    public ElasticQueryBuilder<T> DateRange(string field, DateTime? from = null, DateTime? to = null)
    {
        _configurations.Add(s => s.Query(q => q
            .Range(r => r
                .DateRange(dr =>
                {
                    var range = dr.Field(field);
                    if (from.HasValue) range = range.Gte(DateMath.Anchored(from.Value));
                    if (to.HasValue) range = range.Lte(DateMath.Anchored(to.Value));
                    return range;
                })
            )
        ));
        return this;
    }

    public ElasticQueryBuilder<T> Bool(Action<BoolQueryBuilder<T>> boolConfig)
    {
        var boolBuilder = new BoolQueryBuilder<T>();
        boolConfig(boolBuilder);

        _configurations.Add(s => s.Query(boolBuilder.Build()));
        return this;
    }

    public ElasticQueryBuilder<T> Size(int size)
    {
        _configurations.Add(s => s.Size(size));
        return this;
    }

    public ElasticQueryBuilder<T> From(int from)
    {
        _configurations.Add(s => s.From(from));
        return this;
    }

    public ElasticQueryBuilder<T> Sort(string field, SortOrder order = SortOrder.Asc)
    {
        _configurations.Add(s => s.Sort(sort => sort.Field(field, new FieldSort { Order = order })));
        return this;
    }

    public ElasticQueryBuilder<T> Highlight(params string[] fields)
    {
        _configurations.Add(s => s.Highlight(h =>
        {
            var highlight = h;
            foreach (var field in fields)
            {
                highlight = highlight.Fields(f => f.Add(field, new HighlightField()));
            }
            return highlight;
        }));
        return this;
    }

    public ElasticQueryBuilder<T> Source(params string[] includes)
    {
        _configurations.Add(s => s.Source(src => src.Includes(inc =>
        {
            var includeFields = inc;
            foreach (var field in includes)
            {
                includeFields = includeFields.Field(field);
            }
            return includeFields;
        })));
        return this;
    }

    public ElasticQueryBuilder<T> Aggregation(string name, Action<AggregationDescriptor<T>> aggConfig)
    {
        _configurations.Add(s => s.Aggregations(a =>
        {
            var descriptor = new AggregationDescriptor<T>();
            aggConfig(descriptor);
            return a.Add(name, descriptor);
        }));
        return this;
    }

    public async Task<SearchResponse<T>> ExecuteAsync()
    {
        var request = new SearchRequestDescriptor<T>();

        if (!string.IsNullOrEmpty(_indexName))
        {
            request = request.Index(_indexName);
        }

        foreach (var config in _configurations)
        {
            config(request);
        }

        return await _client.SearchAsync<T>(request);
    }

    public async Task<IEnumerable<T>> GetDocumentsAsync()
    {
        var response = await ExecuteAsync();
        return response.IsValidResponse ? response.Documents : Enumerable.Empty<T>();
    }

    public async Task<(IEnumerable<T> Documents, long TotalCount)> GetResultsAsync()
    {
        var response = await ExecuteAsync();
        if (response.IsValidResponse)
        {
            return (response.Documents, response.Total);
        }
        return (Enumerable.Empty<T>(), 0);
    }
}

// Boolean query builder helper
public class BoolQueryBuilder<T> where T : class
{
    private readonly List<Func<QueryDescriptor<T>, Query>> _mustQueries = new();
    private readonly List<Func<QueryDescriptor<T>, Query>> _mustNotQueries = new();
    private readonly List<Func<QueryDescriptor<T>, Query>> _shouldQueries = new();
    private readonly List<Func<QueryDescriptor<T>, Query>> _filterQueries = new();
    private int? _minimumShouldMatch;

    public BoolQueryBuilder<T> Must(Func<QueryDescriptor<T>, Query> queryFunc)
    {
        _mustQueries.Add(queryFunc);
        return this;
    }

    public BoolQueryBuilder<T> MustNot(Func<QueryDescriptor<T>, Query> queryFunc)
    {
        _mustNotQueries.Add(queryFunc);
        return this;
    }

    public BoolQueryBuilder<T> Should(Func<QueryDescriptor<T>, Query> queryFunc)
    {
        _shouldQueries.Add(queryFunc);
        return this;
    }

    public BoolQueryBuilder<T> Filter(Func<QueryDescriptor<T>, Query> queryFunc)
    {
        _filterQueries.Add(queryFunc);
        return this;
    }

    public BoolQueryBuilder<T> MinimumShouldMatch(int minimum)
    {
        _minimumShouldMatch = minimum;
        return this;
    }

    internal Func<QueryDescriptor<T>, Query> Build()
    {
        return q => q.Bool(b =>
        {
            var boolQuery = b;

            if (_mustQueries.Any())
                boolQuery = boolQuery.Must(_mustQueries.Select(mq => mq(new QueryDescriptor<T>())).ToArray());

            if (_mustNotQueries.Any())
                boolQuery = boolQuery.MustNot(_mustNotQueries.Select(mq => mq(new QueryDescriptor<T>())).ToArray());

            if (_shouldQueries.Any())
                boolQuery = boolQuery.Should(_shouldQueries.Select(sq => sq(new QueryDescriptor<T>())).ToArray());

            if (_filterQueries.Any())
                boolQuery = boolQuery.Filter(_filterQueries.Select(fq => fq(new QueryDescriptor<T>())).ToArray());

            if (_minimumShouldMatch.HasValue)
                boolQuery = boolQuery.MinimumShouldMatch(_minimumShouldMatch.Value);

            return boolQuery;
        });
    }
}

// Advanced query builder with method chaining
public class AdvancedElasticQueryBuilder<T> : ElasticQueryBuilder<T> where T : class
{
    public AdvancedElasticQueryBuilder(ElasticsearchClient client, string indexName = null)
        : base(client, indexName) { }

    public AdvancedElasticQueryBuilder<T> MultiMatch(string query, params string[] fields)
    {
        return (AdvancedElasticQueryBuilder<T>)Bool(b => b
            .Must(q => q.MultiMatch(mm => mm
                .Query(query)
                .Fields(fields)
            ))
        );
    }

    public AdvancedElasticQueryBuilder<T> FuzzySearch(string field, string value, Fuzziness fuzziness = null)
    {
        return (AdvancedElasticQueryBuilder<T>)Bool(b => b
            .Should(q => q.Fuzzy(f => f
                .Field(field)
                .Value(value)
                .Fuzziness(fuzziness ?? Fuzziness.Auto)
            ))
        );
    }

    public AdvancedElasticQueryBuilder<T> Prefix(string field, string prefix)
    {
        return (AdvancedElasticQueryBuilder<T>)Bool(b => b
            .Should(q => q.Prefix(p => p
                .Field(field)
                .Value(prefix)
            ))
        );
    }

    public AdvancedElasticQueryBuilder<T> Wildcard(string field, string pattern)
    {
        return (AdvancedElasticQueryBuilder<T>)Bool(b => b
            .Should(q => q.Wildcard(w => w
                .Field(field)
                .Value(pattern)
            ))
        );
    }

    public AdvancedElasticQueryBuilder<T> TermsIn<TValue>(string field, IEnumerable<TValue> values)
    {
        return (AdvancedElasticQueryBuilder<T>)Bool(b => b
            .Filter(q => q.Terms(t => t
                .Field(field)
                .Terms(new TermsQueryField(values.Select(v => FieldValue.String(v.ToString()))))
            ))
        );
    }

    public AdvancedElasticQueryBuilder<T> Exists(string field)
    {
        return (AdvancedElasticQueryBuilder<T>)Bool(b => b
            .Filter(q => q.Exists(e => e.Field(field)))
        );
    }

    public AdvancedElasticQueryBuilder<T> Missing(string field)
    {
        return (AdvancedElasticQueryBuilder<T>)Bool(b => b
            .MustNot(q => q.Exists(e => e.Field(field)))
        );
    }
}

// Usage examples and helper methods
public class ElasticQueryBuilderExamples
{
    private readonly ElasticsearchClient _client;

    public ElasticQueryBuilderExamples(ElasticsearchClient client)
    {
        _client = client;
    }

    // Example 1: Simple fluent query
    public async Task<IEnumerable<Product>> SimpleQuery()
    {
        var results = await new ElasticQueryBuilder<Product>(_client, "products")
            .Match("name", "laptop")
            .Range("price", from: 100, to: 1000)
            .Size(10)
            .Sort("price", SortOrder.Asc)
            .GetDocumentsAsync();

        return results;
    }

    // Example 2: Complex boolean query
    public async Task<(IEnumerable<Product>, long)> ComplexBooleanQuery(string searchText, string[] categories)
    {
        var results = await new ElasticQueryBuilder<Product>(_client)
            .Index("products")
            .Bool(b => b
                .Should(q => q.Match(m => m.Field("name").Query(searchText)))
                .Should(q => q.Match(m => m.Field("category").Query(searchText)))
                .Filter(q => q.Terms(t => t
                    .Field("category")
                    .Terms(new TermsQueryField(categories.Select(c => FieldValue.String(c))))))
                .Must(q => q.Term(t => t.Field("isActive").Value(true)))
                .MinimumShouldMatch(1)
            )
            .Size(20)
            .From(0)
            .Sort("_score", SortOrder.Desc)
            .Highlight("name", "category")
            .GetResultsAsync();

        return results;
    }

    // Example 3: Advanced search with multiple conditions
    public async Task<IEnumerable<Product>> AdvancedSearch(ProductSearchRequest request)
    {
        var queryBuilder = new AdvancedElasticQueryBuilder<Product>(_client, "products");

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            queryBuilder = queryBuilder.MultiMatch(request.SearchText, "name", "description", "category");
        }

        if (request.Categories?.Any() == true)
        {
            queryBuilder = queryBuilder.TermsIn("category", request.Categories);
        }

        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            queryBuilder = (AdvancedElasticQueryBuilder<Product>)queryBuilder
                .Range("price", request.MinPrice, request.MaxPrice);
        }

        if (request.CreatedAfter.HasValue)
        {
            queryBuilder = (AdvancedElasticQueryBuilder<Product>)queryBuilder
                .DateRange("createdAt", request.CreatedAfter);
        }

        if (request.Tags?.Any() == true)
        {
            queryBuilder = queryBuilder.TermsIn("tags", request.Tags);
        }

        queryBuilder = (AdvancedElasticQueryBuilder<Product>)queryBuilder
            .Size(request.PageSize)
            .From(request.Page * request.PageSize)
            .Sort(request.SortField ?? "_score", request.SortOrder)
            .Source("id", "name", "price", "category", "createdAt");

        return await queryBuilder.GetDocumentsAsync();
    }

    // Example 4: Aggregation query
    public async Task<SearchResponse<Product>> AggregationQuery()
    {
        var response = await new ElasticQueryBuilder<Product>(_client, "products")
            .Size(0) // Only aggregations
            .Aggregation("categories", a => a
                .Terms(t => t.Field("category").Size(10))
            )
            .Aggregation("price_stats", a => a
                .Stats(s => s.Field("price"))
            )
            .ExecuteAsync();

        return response;
    }

    // Example 5: Auto-complete/suggestion query
    public async Task<IEnumerable<Product>> AutoComplete(string input)
    {
        var results = await new AdvancedElasticQueryBuilder<Product>(_client, "products")
            .Bool(b => b
                .Should(q => q.Prefix(p => p.Field("name").Value(input).Boost(3)))
                .Should(q => q.Match(m => m.Field("name").Query(input).Boost(2)))
                .Should(q => q.Wildcard(w => w.Field("name").Value($"*{input}*").Boost(1)))
                .MinimumShouldMatch(1)
            )
            .Size(10)
            .Source("id", "name", "category")
            .GetDocumentsAsync();

        return results;
    }
}

// Helper classes for search requests
public class ProductSearchRequest
{
    public string SearchText { get; set; }
    public string[] Categories { get; set; }
    public string[] Tags { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 20;
    public string SortField { get; set; } = "_score";
    public SortOrder SortOrder { get; set; } = SortOrder.Desc;
}

// Extension methods for even easier usage
public static class ElasticQueryBuilderExtensions
{
    public static ElasticQueryBuilder<T> CreateQuery<T>(this ElasticsearchClient client, string indexName = null)
        where T : class
    {
        return new ElasticQueryBuilder<T>(client, indexName);
    }

    public static AdvancedElasticQueryBuilder<T> CreateAdvancedQuery<T>(this ElasticsearchClient client, string indexName = null)
        where T : class
    {
        return new AdvancedElasticQueryBuilder<T>(client, indexName);
    }
}

// Usage with extension methods
public class ElasticQueryBuilderUsage
{
    public async Task ExampleUsage(ElasticsearchClient client)
    {
        // Simple usage with extension method
        var products = await client
            .CreateQuery<Product>("products")
            .Match("name", "laptop")
            .Range("price", 100, 1000)
            .Size(10)
            .GetDocumentsAsync();

        // Advanced usage
        var searchResults = await client
            .CreateAdvancedQuery<Product>("products")
            .MultiMatch("gaming laptop", "name", "description")
            .TermsIn("category", new[] { "electronics", "computers" })
            .Range("price", 500, 2000)
            .Sort("price", SortOrder.Asc)
            .Highlight("name", "description")
            .GetResultsAsync();
    }
}