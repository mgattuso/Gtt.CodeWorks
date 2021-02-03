using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.AzureStorage
{
    public static class RepositoryExtensions
    {
        public static async Task<T> GetEntity<T>(this CloudTable table, string partitionId, string rowId) where T : class, ITableEntity, new()
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionId, rowId);
            var response = await table.ExecuteAsync(retrieveOperation);
            return response.Result as T;
        }

        public static Task<T> GetEntity<T>(this CloudTable table, Guid partitionId, Guid rowId) where T : class, ITableEntity, new()
        {
            return GetEntity<T>(table, partitionId.ToString(), rowId.ToString());
        }

        public static Task<T> GetEntity<T>(this CloudTable table, Guid partitionId, string rowId) where T : class, ITableEntity, new()
        {
            return GetEntity<T>(table, partitionId.ToString(), rowId);
        }

        public static Task<T> GetEntity<T>(this CloudTable table, string partitionId, Guid rowId) where T : class, ITableEntity, new()
        {
            return GetEntity<T>(table, partitionId, rowId.ToString());
        }

        public static async Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table, TableQuery query)
        {
            TableContinuationToken token = null;
            var retVal = new List<DynamicTableEntity>();
            do
            {
                var results = await table.ExecuteQuerySegmentedAsync(query, token);
                retVal.AddRange(results.Results);
                token = results.ContinuationToken;
            } while (token != null);

            return retVal;
        }

        public static async Task<IEnumerable<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query) where T : ITableEntity, new()
        {
            TableContinuationToken token = null;
            var retVal = new List<T>();
            do
            {
                var results = await table.ExecuteQuerySegmentedAsync(query, token);
                retVal.AddRange(results.Results);
                token = results.ContinuationToken;
            } while (token != null);

            return retVal;
        }

        public static Task<IEnumerable<T>> AllByPartitionKey<T>(this CloudTable table, Guid partitionKey)
            where T : ITableEntity, new()
        {
            return AllByPartitionKey<T>(table, partitionKey.ToString());
        }

        public static Task<IEnumerable<T>> AllByPartitionKey<T>(this CloudTable table, string partitionKey) where T : ITableEntity, new()
        {
            TableQuery<T> query = new TableQuery<T>()
                .Where(TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey));

            return table.ExecuteQueryAsync(query);
        }

        public static TableQuery<T> AndWhere<T>(this TableQuery<T> @this, TableQuery<T> filter) where T : ITableEntity, new()
        {
            return @this.AndWhere(filter.FilterString);
        }

        public static TableQuery<T> AndWhere<T>(this TableQuery<T> @this, string filter) where T : ITableEntity, new()
        {
            @this.FilterString = TableQuery.CombineFilters(@this.FilterString, TableOperators.And, filter);
            return @this;
        }

        public static TableQuery<T> OrWhere<T>(this TableQuery<T> @this, TableQuery<T> filter) where T : ITableEntity, new()
        {
            return @this.OrWhere(filter.FilterString);
        }

        public static TableQuery<T> OrWhere<T>(this TableQuery<T> @this, string filter) where T : ITableEntity, new()
        {
            @this.FilterString = TableQuery.CombineFilters(@this.FilterString, TableOperators.Or, filter);
            return @this;
        }

        public static TableQuery<T> NotWhere<T>(this TableQuery<T> @this, TableQuery<T> filter) where T : ITableEntity, new()
        {
            return @this.NotWhere(filter.FilterString);
        }

        public static TableQuery<T> NotWhere<T>(this TableQuery<T> @this, string filter) where T : ITableEntity, new()
        {
            @this.FilterString = TableQuery.CombineFilters(@this.FilterString, TableOperators.Not, filter);
            return @this;
        }
    }
}
