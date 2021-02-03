using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Gtt.CodeWorks.AzureStorage
{
    public abstract class RepositoryBase
    {
        protected RepositoryBase(string connectionString)
        {
            TableClient = CreateClient(connectionString);
        }

        private static readonly HashSet<string> InitializedTables = new HashSet<string>();

        protected CloudTableClient TableClient { get; }

        private static CloudTableClient CreateClient(string connectionString)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            return cloudStorageAccount.CreateCloudTableClient();
        }

        protected async Task<CloudTable> GetTable(string tableName)
        {
            var table = TableClient.GetTableReference(tableName);
            if (!InitializedTables.Contains(tableName))
            {
                await table.CreateIfNotExistsAsync();
                InitializedTables.Add(tableName);
            }

            return table;
        }

        protected Task<TableResult> InsertDataAsync<T>(CloudTable table, T data) where T : TableEntity
        {
            TableOperation insertOperation = TableOperation.Insert(data);
            return table.ExecuteAsync(insertOperation);
        }

        protected Task<TableResult> InsertOrReplaceDataAsync<T>(CloudTable table, T data) where T : TableEntity
        {
            TableOperation insertOperation = TableOperation.InsertOrReplace(data);
            return table.ExecuteAsync(insertOperation);
        }
    }
}