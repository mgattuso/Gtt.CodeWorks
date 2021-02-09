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

        public async Task<long> StoreStateData<TData, TState>(StateDto metaData, long currentSequenceNumber, TData data) where TData : BaseStateDataModel<TState> where TState : struct, IConvertible
        {
            var tableName = metaData.MachineName.Replace(".", "");
            var table = await GetTable($"State{tableName}");

            string partitionKey = metaData.Identifier;

            var currentMarker = await table.GetEntity<StateDataTable>(partitionKey, "current");

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

            currentMarker.PartitionKey = partitionKey;
            currentMarker.RowKey = "current";
            currentMarker.SequenceNumber = nextSequenceNumber;
            currentMarker.MachineName = metaData.MachineName;
            currentMarker.ContentLength = serializedState.Length;
            currentMarker.SerializedState = serializedState;
            currentMarker.Source = metaData.Source;
            currentMarker.Destination = metaData.Destination;
            currentMarker.IsReentry = metaData.IsReentry;
            currentMarker.Trigger = metaData.Trigger;
            currentMarker.UpdateAuditable(metaData);

            var record = new StateDataTable
            {
                PartitionKey = partitionKey,
                RowKey = nextSequenceNumber.ToString(),
                MachineName = metaData.MachineName,
                Source = metaData.Source,
                ContentLength = serializedState.Length,
                Destination = metaData.Destination,
                IsReentry = metaData.IsReentry,
                SequenceNumber = nextSequenceNumber,
                SerializedState = serializedState,
                Trigger = metaData.Trigger
            }.UpdateAuditable(metaData);

            try
            {
                TableBatchOperation batch = new TableBatchOperation
                {
                    TableOperation.Insert(record),
                    TableOperation.InsertOrReplace(currentMarker)
                };
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

        public async Task<StoredState<TData, TState>> RetrieveStateData<TData, TState>(string identifier, string machineName, long? version) where TData : BaseStateDataModel<TState> where TState : struct, IConvertible
        {
            var tableName = machineName.Replace(".", "");
            var table = await GetTable($"State{tableName}");

            version = version == 0 ? null : version;

            string partitionKey = identifier;

            if (version < 0)
            {
                var q = TableOperation.Retrieve(partitionKey, "current", new List<string>() { "SequenceNumber" });
                var res = await table.ExecuteAsync(q);
                var te = res.Result as DynamicTableEntity;
                if (te != null)
                {
                    long? current = te.Properties["SequenceNumber"].Int64Value;
                    version = current.HasValue ? current + version : version;
                }
            }

            
            string versionToRetrieve = version?.ToString() ?? "current";
            var data = await table.GetEntity<StateDataTable>(partitionKey, versionToRetrieve);

            if (data == null)
            {
                return null;
            }

            var state = new StateDto
            {
                Identifier = data.PartitionKey,
                MachineName = data.MachineName,
                Source = data.Source,
                Destination = data.Destination,
                IsReentry = data.IsReentry,
                SequenceNumber = data.SequenceNumber,
                Trigger = data.Trigger,
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
