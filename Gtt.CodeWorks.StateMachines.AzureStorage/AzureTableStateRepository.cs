using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Gtt.CodeWorks.AzureStorage;
using Microsoft.Extensions.Logging;

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
            TableClient table = await GetTable(tableName);
            string partitionKey = string.IsNullOrWhiteSpace(parentIdentifier) ? metaData.Identifier : parentIdentifier;
            string rowKeyPrefix = string.IsNullOrWhiteSpace(parentIdentifier) ? "" : $"{metaData.Identifier}-";

            var currentResponse = await table.GetEntityAsync<StateDataTable>(partitionKey, rowKeyPrefix + "current");
            StateDataTable currentMarker = currentResponse?.Value;

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
                List<TableTransactionAction> batch = new List<TableTransactionAction>();

                batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, currentMarker));

                if (saveHistory)
                {
                    batch.Add(new TableTransactionAction(TableTransactionActionType.Add, record));
                }

                await table.SubmitTransactionAsync(batch);
            }
            catch (Azure.Data.Tables.TableTransactionFailedException ex)
            {
                _logger.LogError($"StatusCode={ex.Status}, HttpMessage={ex.ErrorCode}");
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
                var res = await table.GetEntityAsync<TableEntity>(partitionKey, rowKeyPrefix + "current", new List<string>() { "SequenceNumber" });
                var te = res.Value;
                if (te != null)
                {
                    long? current = te.GetInt64("SequenceNumber");
                    version = current.HasValue ? current + version : version;
                }
            }


            string versionToRetrieve = rowKeyPrefix + (version?.ToString() ?? "current");
            var dataResponse = await table.GetEntityAsync<StateDataTable>(partitionKey, versionToRetrieve);

            if (dataResponse.Value == null)
            {
                return null;
            }

            var data = dataResponse.Value;

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
