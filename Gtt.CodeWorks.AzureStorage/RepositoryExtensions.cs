using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.AzureStorage
{
    public static class RepositoryExtensions
    {
        public static async Task<T> GetEntity<T>(this TableClient table, string partitionId, string rowId) where T : class, ITableEntity, new()
        {
            var result = await table.GetEntityAsync<T>(partitionId, rowId);
            return result?.Value;
        }

        public static Task<T> GetEntity<T>(this TableClient table, Guid partitionId, Guid rowId) where T : class, ITableEntity, new()
        {
            return GetEntity<T>(table, partitionId.ToString(), rowId.ToString());
        }

        public static Task<T> GetEntity<T>(this TableClient table, Guid partitionId, string rowId) where T : class, ITableEntity, new()
        {
            return GetEntity<T>(table, partitionId.ToString(), rowId);
        }

        public static Task<T> GetEntity<T>(this TableClient table, string partitionId, Guid rowId) where T : class, ITableEntity, new()
        {
            return GetEntity<T>(table, partitionId, rowId.ToString());
        }

        //public static async Task<IEnumerable<TableEntity>> ExecuteQueryAsync(this TableClient table, TableQuery query)
        //{
        //    var retVal = new List<TableEntity>();
        //    do
        //    {

        //        var results = await table.QueryAsync
        //        retVal.AddRange(results);
        //    } while (token != null);

        //    return retVal;
        //}

        //public static async Task<IEnumerable<T>> ExecuteQueryAsync<T>(this TableClient table, TableQuery<T> query) where T : ITableEntity, new()
        //{
        //    TableContinuationToken token = null;
        //    var retVal = new List<T>();
        //    do
        //    {
        //        var results = await table.ExecuteQuerySegmentedAsync(query, token);
        //        retVal.AddRange(results.Results);
        //        token = results.ContinuationToken;
        //    } while (token != null);

        //    return retVal;
        //}

        public static Task<IEnumerable<T>> AllByPartitionKey<T>(this TableClient table, Guid partitionKey)
            where T : class, ITableEntity, new()
        {
            return AllByPartitionKey<T>(table, partitionKey.ToString());
        }

        public async static Task<IEnumerable<T>> AllByPartitionKey<T>(this TableClient table, string partitionKey) where T : class, ITableEntity, new()
        {
            var query = table.QueryAsync<T>(filter: $"PartitionKey eq '{partitionKey}'");
            List<T> list = new List<T>();
            string continuationToken = null;

            do
            {
                var results = query.AsPages(continuationToken);
                await foreach (var r in results)
                {
                    list.AddRange(r.Values);
                }

            } while (continuationToken != null);
            return list;
        }
    }
}
