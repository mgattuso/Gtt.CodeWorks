using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gtt.CodeWorks.AzureStorage;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Gtt.CodeWorks.StateMachines.AzureStorage
{
    public class AzureTableStateRepository : RepositoryBase, IStateRepository
    {
        private readonly IObjectSerializer _objectSerializer;
        private readonly ILogger<AzureTableStateRepository> _logger;

        public AzureTableStateRepository(
            string connectionString,
            IObjectSerializer objectSerializer,
            ILogger<AzureTableStateRepository> logger) : base(connectionString)
        {
            _objectSerializer = objectSerializer;
            _logger = logger;
        }

        public async Task<long> StoreStateData<TData, TState>(StateDto metaData, long currentSequenceNumber, TData data, bool saveHistory, string parentIdentifier)
            where TData : BaseStateDataModel<TState>
            where TState : struct, IConvertible
        {
            var tableName = NormalizeTableName(metaData.MachineName);
            _logger.LogTrace($"Using table name {tableName}");
            var table = await GetTable(tableName);
            string partitionKey = string.IsNullOrWhiteSpace(parentIdentifier) ? metaData.Identifier : parentIdentifier;
            string rowKeyPrefix = string.IsNullOrWhiteSpace(parentIdentifier) ? "" : $"{metaData.Identifier}-";

            var currentMarker = await table.GetEntity<StateDataTable>(partitionKey, rowKeyPrefix+"current");

            long sequenceNumber = currentMarker?.SequenceNumber ?? 0;

            if (sequenceNumber != currentSequenceNumber)
            {
                throw new Exception($"Out of sequence state update attempted. For {metaData.Identifier} attempted to replace {currentSequenceNumber}. Current version is actually {sequenceNumber}");
            }

            long nextSequenceNumber = sequenceNumber + 1;

            if (currentMarker == null)
            {
                currentMarker = new StateDataTable();
            }

            var serializedState = await _objectSerializer.Serialize(data);

            if (serializedState.Length > 28_000)
            {
                throw new Exception("Serialized data is > 28,000 chars");
            }

            currentMarker.PartitionKey = partitionKey;
            currentMarker.RowKey = rowKeyPrefix + "current";
            currentMarker.SequenceNumber = nextSequenceNumber;
            currentMarker.MachineName = metaData.MachineName;
            currentMarker.ContentLength = serializedState.Length;
            currentMarker.SerializedState = serializedState;
            currentMarker.Source = metaData.Source;
            currentMarker.Destination = metaData.Destination;
            currentMarker.IsReentry = metaData.IsReentry;
            currentMarker.Trigger = metaData.Trigger;
            currentMarker.CorrelationId = metaData.CorrelationId.ToString();
            currentMarker.UserIdentifier = metaData.UserIdentifier;
            currentMarker.Username = metaData.Username;
            currentMarker.UpdateAuditable(metaData);

            var record = new StateDataTable
            {
                PartitionKey = partitionKey,
                RowKey = rowKeyPrefix + nextSequenceNumber,
                MachineName = metaData.MachineName,
                Source = metaData.Source,
                ContentLength = serializedState.Length,
                Destination = metaData.Destination,
                IsReentry = metaData.IsReentry,
                SequenceNumber = nextSequenceNumber,
                SerializedState = serializedState,
                Trigger = metaData.Trigger,
                UserIdentifier = metaData.UserIdentifier,
                Username = metaData.Username,
                CorrelationId = metaData.CorrelationId.ToString()
            }.UpdateAuditable(metaData);

            try
            {
                TableBatchOperation batch = new TableBatchOperation
                {
                    TableOperation.InsertOrReplace(currentMarker)
                };

                if (saveHistory)
                {
                    batch.Add(TableOperation.Insert(record));
                }

                await table.ExecuteBatchAsync(batch);
            }
            catch (StorageException ex)
            {
                var ri = ex.RequestInformation;
                _logger.LogError($"StatusCode={ri.HttpStatusCode}, HttpMessage={ri.HttpStatusMessage}");
                throw;
            }
            catch (Exception ex)
            {
                var t = ex.GetType();
                _logger.LogError(ex, "Error calling storage");
                throw;
            }

            return nextSequenceNumber;
        }

        private static string NormalizeTableName(string table)
        {
            var tableName = table.Replace(".", "");
            if (tableName.Length > 62)
            {
                return tableName.Substring(tableName.Length - 62, 62);
            }

            return $"S{tableName}";
        }

        public async Task<StoredState<TData, TState>> RetrieveStateData<TData, TState>(string identifier, string machineName, long? version, string parentIdentifier) where TData : BaseStateDataModel<TState> where TState : struct, IConvertible
        {
            var tableName = NormalizeTableName(machineName);
            _logger.LogTrace($"Using table name {tableName}");
            var table = await GetTable(tableName);

            version = version == 0 ? null : version;
            string partitionKey = string.IsNullOrWhiteSpace(parentIdentifier) ? identifier : parentIdentifier;
            string rowKeyPrefix = string.IsNullOrWhiteSpace(parentIdentifier) ? "" : $"{identifier}-";

            if (version < 0)
            {
                var q = TableOperation.Retrieve(partitionKey, rowKeyPrefix+"current", new List<string>() { "SequenceNumber" });
                var res = await table.ExecuteAsync(q);
                var te = res.Result as DynamicTableEntity;
                if (te != null)
                {
                    long? current = te.Properties["SequenceNumber"].Int64Value;
                    version = current.HasValue ? current + version : version;
                }
            }


            string versionToRetrieve = rowKeyPrefix + (version?.ToString() ?? "current");
            var data = await table.GetEntity<StateDataTable>(partitionKey, versionToRetrieve);

            if (data == null)
            {
                return null;
            }

            int rowKeySplitPoint = data.RowKey.LastIndexOf("-", StringComparison.Ordinal);

            string id = string.IsNullOrWhiteSpace(parentIdentifier)
                ? data.PartitionKey
                : data.RowKey.Substring(0, rowKeySplitPoint);

            var state = new StateDto
            {
                Identifier = id,
                MachineName = data.MachineName,
                Source = data.Source,
                Destination = data.Destination,
                IsReentry = data.IsReentry,
                SequenceNumber = data.SequenceNumber,
                Trigger = data.Trigger,
                Username = data.Username,
                UserIdentifier = data.UserIdentifier,
                CorrelationId = data.CorrelationId.ToGuid(),
                ParentIdentifier = parentIdentifier
            }.UpdateAuditable(data);

            var obj = await _objectSerializer.Deserialize<TData>(data.SerializedState);

            return new StoredState<TData, TState>
            {
                StateMetaData = state,
                Data = obj
            };
        }
    }
}
