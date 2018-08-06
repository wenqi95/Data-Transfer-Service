using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UserManagement.Services
{
    public class CosmosDBManager<T>
    {
        private DocumentClient client = null;
        private const string endpointUrl = "https://iotdata.documents.azure.cn:443/";
        private const string primaryKey = "ofsc2eJy8lmZs8tnpAKyQdeJzzmu8XkWzuE6ciMJhGTey9T2FkWiP2COO9nO2UCaXYuRmPOW0VlqHlyXCjx0kQ==";
        private const string databaseId = "iotdataid";
        private const string collectionId = "iotcollectionId";

        public CosmosDBManager()
        {
            client = new DocumentClient(new Uri(endpointUrl), primaryKey);
            InitializeDatabaseAsync().Wait();
        }

        private async Task InitializeDatabaseAsync()
        { 
            await client.CreateDatabaseIfNotExistsAsync(new Database() { Id = databaseId });
            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseId), new DocumentCollection()
            {
                Id = collectionId
            });
        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                Document document =
                    await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default(T);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<Document> CreateItemAsync(T item)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), item);
        }

        public async Task<Document> UpdateItemAsync(string id, T item)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id), item);
        }

        public async Task DeleteItemAsync(string id)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));
        }
    }
}
