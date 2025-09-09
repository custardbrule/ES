using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;

// Setup client
var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    .DefaultIndex("my-index")
    .Authentication(new BasicAuthentication("username", "password")); // if needed

var client = new ElasticsearchClient(settings);

// Example document
public record Product
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Category { get; init; }
    public decimal Price { get; init; }
    public string[] Tags { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsActive { get; init; }
}

public class ElasticsearchQueryExamples
{
    private readonly ElasticsearchClient _client;

    public ElasticsearchQueryExamples(ElasticsearchClient client)
    {
        _client = client;
    }

    // 1. Simple match query
    public async Task<SearchResponse<Product>> SimpleMatchQuery(string searchText)
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query(searchText)
                )
            )
        );
        return response;
    }

    // 2. Multi-field search
    public async Task<SearchResponse<Product>> MultiFieldSearch(string searchText)
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(searchText)
                    .Fields(f => f
                        .Field(x => x.Name, boost: 2.0)
                        .Field(x => x.Category)
                        .Field(x => x.Tags)
                    )
                    .Type(TextQueryType.BestFields)
                    .Fuzziness(Fuzziness.Auto)
                )
            )
        );
        return response;
    }

    // 3. Boolean query with multiple conditions
    public async Task<SearchResponse<Product>> BooleanQuery(string category, decimal minPrice, decimal maxPrice)
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .Term(t => t
                            .Field(f => f.Category)
                            .Value(category)
                        )
                    )
                    .Must(m => m
                        .Range(r => r
                            .NumberRange(nr => nr
                                .Field(f => f.Price)
                                .Gte(minPrice)
                                .Lte(maxPrice)
                            )
                        )
                    )
                    .Should(sh => sh
                        .Term(t => t
                            .Field(f => f.IsActive)
                            .Value(true)
                        )
                    )
                    .MustNot(mn => mn
                        .Terms(t => t
                            .Field(f => f.Tags)
                            .Terms(new TermsQueryField(new[] { "discontinued", "expired" }))
                        )
                    )
                )
            )
            .Size(20)
            .From(0)
        );
        return response;
    }

    // 4. Aggregations
    public async Task<SearchResponse<Product>> AggregationQuery()
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Size(0) // Only return aggregations
            .Aggregations(a => a
                // Terms aggregation
                .Terms("categories", t => t
                    .Field(f => f.Category)
                    .Size(10)
                )
                // Stats aggregation
                .Stats("price_stats", st => st
                    .Field(f => f.Price)
                )
                // Date histogram
                .DateHistogram("created_over_time", dh => dh
                    .Field(f => f.CreatedAt)
                    .CalendarInterval(CalendarInterval.Month)
                )
                // Nested aggregation
                .Terms("category_price_ranges", t => t
                    .Field(f => f.Category)
                    .Aggregations(nested => nested
                        .Range("price_ranges", r => r
                            .Field(f => f.Price)
                            .Ranges(ranges => ranges
                                .To(50)
                                .From(50).To(100)
                                .From(100)
                            )
                        )
                    )
                )
            )
        );
        return response;
    }

    // 5. Complex filtered search with sorting
    public async Task<SearchResponse<Product>> ComplexFilteredSearch(
        string searchText,
        string[] categories,
        string[] tags,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? createdAfter = null)
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Bool(b =>
                {
                    var boolQuery = b.Must(m => m
                        .Bool(innerBool => innerBool
                            .Should(should =>
                            {
                                var shouldQuery = should;

                                if (!string.IsNullOrEmpty(searchText))
                                {
                                    shouldQuery = shouldQuery.MultiMatch(mm => mm
                                        .Query(searchText)
                                        .Fields(f => f.Field(x => x.Name).Field(x => x.Category))
                                    );
                                }

                                return shouldQuery;
                            })
                            .MinimumShouldMatch(1)
                        )
                    );

                    // Add filters
                    if (categories?.Any() == true)
                    {
                        boolQuery = boolQuery.Filter(f => f
                            .Terms(t => t
                                .Field(x => x.Category)
                                .Terms(new TermsQueryField(categories))
                            )
                        );
                    }

                    if (tags?.Any() == true)
                    {
                        boolQuery = boolQuery.Filter(f => f
                            .Terms(t => t
                                .Field(x => x.Tags)
                                .Terms(new TermsQueryField(tags))
                            )
                        );
                    }

                    if (minPrice.HasValue || maxPrice.HasValue)
                    {
                        boolQuery = boolQuery.Filter(f => f
                            .Range(r => r
                                .NumberRange(nr =>
                                {
                                    var rangeQuery = nr.Field(x => x.Price);
                                    if (minPrice.HasValue) rangeQuery = rangeQuery.Gte(minPrice.Value);
                                    if (maxPrice.HasValue) rangeQuery = rangeQuery.Lte(maxPrice.Value);
                                    return rangeQuery;
                                })
                            )
                        );
                    }

                    if (createdAfter.HasValue)
                    {
                        boolQuery = boolQuery.Filter(f => f
                            .Range(r => r
                                .DateRange(dr => dr
                                    .Field(x => x.CreatedAt)
                                    .Gte(DateMath.Anchored(createdAfter.Value))
                                )
                            )
                        );
                    }

                    return boolQuery;
                })
            )
            .Sort(sort => sort
                .Score(score => score.Order(SortOrder.Desc))
                .Field(f => f.CreatedAt, new FieldSort { Order = SortOrder.Desc })
            )
            .Size(20)
            .From(0)
            .Highlight(h => h
                .Fields(fields => fields
                    .Add(x => x.Name, new HighlightField())
                    .Add(x => x.Category, new HighlightField())
                )
            )
        );
        return response;
    }

    // 6. Suggestions and autocomplete
    public async Task<SearchResponse<Product>> AutocompleteQuery(string input)
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Bool(b => b
                    .Should(should => should
                        .Prefix(p => p
                            .Field(f => f.Name)
                            .Value(input)
                            .Boost(2.0)
                        )
                    )
                    .Should(should => should
                        .Wildcard(w => w
                            .Field(f => f.Name)
                            .Value($"*{input}*")
                            .Boost(1.0)
                        )
                    )
                    .Should(should => should
                        .Fuzzy(fz => fz
                            .Field(f => f.Name)
                            .Value(input)
                            .Fuzziness(Fuzziness.One)
                            .Boost(0.5)
                        )
                    )
                    .MinimumShouldMatch(1)
                )
            )
            .Size(10)
            .Source(src => src
                .Includes(inc => inc
                    .Field(f => f.Id)
                    .Field(f => f.Name)
                    .Field(f => f.Category)
                )
            )
        );
        return response;
    }

    // 7. Faceted search
    public async Task<SearchResponse<Product>> FacetedSearch(
        string searchText,
        Dictionary<string, string[]> selectedFacets)
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q =>
            {
                var boolQuery = q.Bool(b => b);

                // Main search query
                if (!string.IsNullOrEmpty(searchText))
                {
                    boolQuery = q.Bool(b => b
                        .Must(m => m
                            .MultiMatch(mm => mm
                                .Query(searchText)
                                .Fields(f => f.Field(x => x.Name).Field(x => x.Category))
                            )
                        )
                    );
                }

                // Apply selected facet filters
                foreach (var facet in selectedFacets)
                {
                    boolQuery = q.Bool(b => b
                        .Must(boolQuery.Bool.Must.ToArray())
                        .Filter(f => f
                            .Terms(t => t
                                .Field(facet.Key)
                                .Terms(new TermsQueryField(facet.Value))
                            )
                        )
                    );
                }

                return boolQuery;
            })
            .Aggregations(aggs => aggs
                .Terms("categories", t => t
                    .Field(f => f.Category)
                    .Size(20)
                )
                .Terms("tags", t => t
                    .Field(f => f.Tags)
                    .Size(50)
                )
                .Range("price_ranges", r => r
                    .Field(f => f.Price)
                    .Ranges(ranges => ranges
                        .To(25).Key("Under $25")
                        .From(25).To(50).Key("$25 - $50")
                        .From(50).To(100).Key("$50 - $100")
                        .From(100).Key("Over $100")
                    )
                )
            )
            .Size(20)
        );
        return response;
    }
}

// Usage example
public class ElasticsearchService
{
    private readonly ElasticsearchQueryExamples _queries;

    public ElasticsearchService(ElasticsearchClient client)
    {
        _queries = new ElasticsearchQueryExamples(client);
    }

    public async Task<(IEnumerable<Product> Results, long TotalCount)> SearchProducts(
        string searchText,
        string[] categories = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        var response = await _queries.ComplexFilteredSearch(
            searchText, categories, null, minPrice, maxPrice);

        if (response.IsValidResponse)
        {
            var products = response.Documents;
            var totalCount = response.Total;
            return (products, totalCount);
        }

        throw new Exception($"Elasticsearch query failed: {response.DebugInformation}");
    }

    public async Task<Dictionary<string, long>> GetCategoryFacets(string searchText = null)
    {
        var response = await _queries.FacetedSearch(searchText, new Dictionary<string, string[]>());

        if (response.IsValidResponse && response.Aggregations != null)
        {
            var categoryAgg = response.Aggregations.GetStringTerms("categories");
            return categoryAgg.Buckets.ToDictionary(
                bucket => bucket.Key.Value,
                bucket => bucket.DocCount);
        }

        return new Dictionary<string, long>();
    }
}