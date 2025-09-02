using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public interface IElasticSearchContext
    {
        ElasticsearchClient Client { get; }
        Task<IndexResponse> IndexAsync<T>(string indexName, string documentId, T document) where T : class;
        Task<UpdateResponse<T>> UpsertAsync<T>(string indexName, string documentId, object partialUpdate, T upsertDocument) where T : class;
        Task<DeleteResponse> DeleteAsync(string indexName, string documentId);
        Task<BulkResponse> BulkIndexAsync<T>(string indexName, IEnumerable<T> documents, Func<T, string> getDocumentId) where T : class;
        Task<BulkResponse> BulkUpsertAsync<T>(string indexName, IEnumerable<T> documents, Func<T, string> getDocumentId) where T : class;
        Task<BulkResponse> BulkUpsertWithPartialAsync<T, TPartial>(string indexName, IEnumerable<(string Id, TPartial PartialDoc)> updates)
            where T : class
            where TPartial : class;
        Task<BulkResponse> BulkDeleteAsync<T>(string indexName, IEnumerable<string> documentIds) where T : class;
        Task<DeleteByQueryResponse> BulkDeleteByQueryAsync<T>(string indexName, Query query) where T : class;

    }

    public class ElasticSearchContextOptions
    {
        public string CertPath { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class ElasticSearchContext : IElasticSearchContext
    {
        private readonly ElasticsearchClient _client;

        public ElasticSearchContext(ElasticsearchClient client)
        {
            _client = client;
        }
        public ElasticsearchClient Client => _client;

        /// <summary>
        /// Performs index document
        /// </summary>
        /// <typeparam name="T">Document type</typeparam>
        /// <param name="indexName">Target index name</param>
        /// <param name="documentId">Document id to index</param>
        /// <param name="document">Document to index</param>
        /// <returns>Index response</returns>
        public Task<IndexResponse> IndexAsync<T>(string indexName, string documentId, T document)
        where T : class
        {
            var indexRequest = new IndexRequest<T>(document, indexName, documentId);

            return Client.IndexAsync(indexRequest);
        }

        /// <summary>
        /// Performs Upsert document
        /// </summary>
        /// <typeparam name="T">Document type</typeparam>
        /// <param name="indexName">Target index name</param>
        /// <param name="documentId">Document id to update</param>
        /// <param name="partialUpdate">Document to partial update</param>
        /// <param name="upsertDocument">Document to upsert</param>
        /// <returns>Update response</returns>
        public Task<UpdateResponse<T>> UpsertAsync<T>(string indexName, string documentId, object partialUpdate, T upsertDocument)
        where T : class
        {
            var updateRequest = new UpdateRequest<T, object>(indexName, documentId)
            {
                Doc = partialUpdate,
                Upsert = upsertDocument
            };

            return Client.UpdateAsync(updateRequest);
        }

        /// <summary>
        /// Performs Delete document
        /// </summary>
        /// <typeparam name="T">Document type</typeparam>
        /// <param name="indexName">Target index name</param>
        /// <param name="documentId">Document id to delete</param>
        /// <returns>Delete response</returns>
        public Task<DeleteResponse> DeleteAsync(string indexName, string documentId)
        {
            var deleteRequest = new DeleteRequest(indexName, documentId);
            return _client.DeleteAsync(deleteRequest);
        }


        /// <summary>
        /// Performs bulk upsert using index operations (alternative approach)
        /// </summary>
        /// <typeparam name="T">Document type</typeparam>
        /// <param name="indexName">Target index name</param>
        /// <param name="documents">Documents to upsert</param>
        /// <param name="getDocumentId">Function to extract document ID</param>
        /// <returns>Bulk response with operation results</returns>
        public Task<BulkResponse> BulkIndexAsync<T>(
            string indexName,
            IEnumerable<T> documents,
            Func<T, string> getDocumentId) where T : class
        {
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentException("Index name cannot be null or empty", nameof(indexName));

            if (documents == null || !documents.Any()) throw new ArgumentException("Documents collection cannot be null or empty", nameof(documents));

            if (getDocumentId == null) throw new ArgumentNullException(nameof(getDocumentId));

            var idxOperations = documents.Select(document =>
            {
                var documentId = getDocumentId(document);
                if (string.IsNullOrEmpty(documentId)) throw new InvalidOperationException("Document ID cannot be null or empty");

                // Using index operation which will create or replace the document
                return new BulkIndexOperation<T>(document, indexName);
            });

            var bulkRequest = new BulkRequest
            {
                Operations = new List<IBulkOperation>(idxOperations)
            };

            return Client.BulkAsync(bulkRequest);
        }

        /// <summary>
        /// Performs bulk upsert operation on Elasticsearch documents
        /// </summary>
        /// <typeparam name="T">Document type</typeparam>
        /// <param name="indexName">Target index name</param>
        /// <param name="documents">Documents to upsert</param>
        /// <param name="getDocumentId">Function to extract document ID</param>
        /// <returns>Bulk response with operation results</returns>
        public Task<BulkResponse> BulkUpsertAsync<T>(
            string indexName,
            IEnumerable<T> documents,
            Func<T, string> getDocumentId) where T : class
        {
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentException("Index name cannot be null or empty", nameof(indexName));

            if (documents == null || !documents.Any()) throw new ArgumentException("Documents collection cannot be null or empty", nameof(documents));

            if (getDocumentId == null) throw new ArgumentNullException(nameof(getDocumentId));

            var bulkRequest = new BulkRequest(indexName)
            {
                Operations = new List<IBulkOperation>(documents.Select(document =>
                {
                    var documentId = getDocumentId(document);
                    if (string.IsNullOrEmpty(documentId)) throw new InvalidOperationException("Document ID cannot be null or empty");

                    // Create an update operation with upsert
                    return new BulkUpdateOperation<T, T>(documentId)
                    {
                        Doc = document,
                        DocAsUpsert = true // This enables upsert behavior
                    };
                }))
            };

            return Client.BulkAsync(bulkRequest);
        }

        /// Performs bulk upsert with custom partial update document
        /// </summary>
        /// <typeparam name="TPartial">Partial update document type</typeparam>
        /// <param name="indexName">Target index name</param>
        /// <param name="updates">Update operations with ID and partial documents</param>
        /// <returns>Bulk response with operation results</returns>
        public Task<BulkResponse> BulkUpsertWithPartialAsync<T, TPartial>(
            string indexName,
            IEnumerable<(string Id, TPartial PartialDoc)> updates)
            where T : class
            where TPartial : class
        {
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentException("Index name cannot be null or empty", nameof(indexName));

            if (updates == null || !updates.Any()) throw new ArgumentException("Updates collection cannot be null or empty", nameof(updates));

            var bulkRequest = new BulkRequest(indexName)
            {
                Operations = new List<IBulkOperation>(updates.Select(v =>
                {
                    var (id, partialDoc) = v;
                    if (string.IsNullOrEmpty(id)) throw new InvalidOperationException("Document ID cannot be null or empty");

                    return BulkUpdateOperationFactory.WithPartial(id, partialDoc);
                }))
            };

            return Client.BulkAsync(bulkRequest);
        }

        /// <summary>
        /// Performs bulk delete operation on Elasticsearch documents
        /// </summary>
        /// <typeparam name="T">Document type</typeparam>
        /// <param name="indexName">Target index name</param>
        /// <param name="documentIds">Documents to delete</param>
        /// <returns>Bulk response with operation results</returns>
        public Task<BulkResponse> BulkDeleteAsync<T>(
            string indexName,
            IEnumerable<string> documentIds) where T : class
        {
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentException("Index name cannot be null or empty", nameof(indexName));

            var bulkRequest = new BulkRequest(indexName)
            {
                Operations = new List<IBulkOperation>(documentIds.Select(id => new BulkDeleteOperation<T>(id) { Index = indexName }))
            };

            return Client.BulkAsync(bulkRequest);
        }

        /// <summary>
        /// Performs bulk delete operation on Elasticsearch documents
        /// </summary>
        /// <typeparam name="T">Document type</typeparam>
        /// <param name="indexName">Target index name</param>
        /// <param name="query">Documents to delete</param>
        /// <returns>Bulk response with operation results</returns>
        public Task<DeleteByQueryResponse> BulkDeleteByQueryAsync<T>(string indexName, Query query)
        where T : class
        {
            var deleteByQueryRequest = new DeleteByQueryRequest(indexName)
            {
                Query = query,
                Conflicts = Conflicts.Proceed,
                WaitForCompletion = true
            };

            return Client.DeleteByQueryAsync(deleteByQueryRequest);
        }
    }
}
