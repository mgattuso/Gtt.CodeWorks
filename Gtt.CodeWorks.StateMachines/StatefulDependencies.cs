using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.StateMachines
{
    public class StatefulDependencies
    {
        public IStateRepository StateRepository { get; }

        public StatefulDependencies(IStateRepository stateRepository)
        {
            StateRepository = stateRepository;
        }

        public static StatefulDependencies Default => new StatefulDependencies(new InMemoryStateRepository());
    }

    public class InMemoryStateRepository : IStateRepository
    {
        private Dictionary<Tuple<long, string, string>, Tuple<StateDto, object>> _data = new Dictionary<Tuple<long, string, string>, Tuple<StateDto, object>>();

        public async Task<long> StoreStateData<TData, TState>(StateDto metaData, long currentSequenceNumber, TData data) where TData : BaseStateDataModel<TState> where TState : struct, IConvertible
        {
            var existingEntry = await RetrieveStateData<TData, TState>(metaData.Identifier, metaData.MachineName);
            long sequenceNumber = existingEntry?.StateMetaData?.SequenceNumber ?? 0;
            if (sequenceNumber != currentSequenceNumber)
            {
                throw new Exception($"Out of sequence state update attempted. For {metaData.Identifier} attempted to replace {currentSequenceNumber}. Current version is actually {sequenceNumber}");
            }

            long nextSequenceNumber = sequenceNumber + 1;
            StateDto copyOfMetaData = metaData.Copy();
            object copyOfData = data.Copy();
            copyOfMetaData.SequenceNumber = nextSequenceNumber;
            _data[Tuple.Create(nextSequenceNumber, metaData.Identifier, metaData.MachineName)] = Tuple.Create(copyOfMetaData, copyOfData);
            return nextSequenceNumber;
        }

        public Task<StoredState<TData, TState>> RetrieveStateData<TData, TState>(string identifier, string machineName) where TData : BaseStateDataModel<TState> where TState : struct, IConvertible
        {
            return RetrieveStateData<TData, TState>(identifier, machineName, null);
        }

        public async Task<StoredState<TData, TState>> RetrieveStateData<TData, TState>(string identifier, string machineName, long? sequenceNumber) where TData : BaseStateDataModel<TState> where TState : struct, IConvertible
        {
            await Task.CompletedTask;
            if (_data.Count == 0)
            {
                return null;
            }

            var q = _data.Where(x => x.Key.Item2 == identifier && x.Key.Item3 == machineName);

            if (sequenceNumber != null)
            {
                q = q.Where(x => x.Key.Item1 == sequenceNumber.Value);
            }
            
            var r = q.OrderByDescending(x => x.Key.Item1).FirstOrDefault();
            if (r.Key?.Item1 == null || r.Key.Item1 == 0)
            {
                return null;
            }

            var v = new StoredState<TData, TState>
            {
                Data = r.Value.Item2 as TData,
                StateMetaData = r.Value.Item1
            };

            return v;
        }
    }
}
