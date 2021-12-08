using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace Gtt.CodeWorks.AzureStorage
{
    public abstract class RepositoryBase
    {
        protected RepositoryBase(string connectionString)
        {
            TableClient = CreateClient(connectionString);
        }

        private static readonly HashSet<string> InitializedTables = new HashSet<string>();

        protected TableServiceClient TableClient { get; }

        private static TableServiceClient CreateClient(string connectionString)
        {
            var cloudStorageAccount = new TableServiceClient(connectionString);
            return cloudStorageAccount;
        }

        protected async Task<TableClient> GetTable(string tableName)
        {
            var table = TableClient.GetTableClient(tableName);
            if (!InitializedTables.Contains(tableName))
            {
                await table.CreateIfNotExistsAsync();
                InitializedTables.Add(tableName);
            }

            return table;
        }

        protected Task<Azure.Response> InsertDataAsync<T>(TableClient table, T data) where T : class, ITableEntity, new()
        {
            return table.AddEntityAsync(data);
        }

        protected Task<Azure.Response> InsertOrReplaceDataAsync<T>(TableClient table, T data) where T : class, ITableEntity, new()
        {
            return table.UpsertEntityAsync(data);
        }
    }
}