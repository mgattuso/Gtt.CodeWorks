using System;
using System.Threading.Tasks;
using Stateless.Graph;

namespace Gtt.CodeWorks.StateMachines
{
    public interface IStateRepository
    {
        Task<long> StoreStateData<TData, TState>(StateDto metaData, long currentSequenceNumber, TData data, bool saveHistory)
            where TData : BaseStateDataModel<TState>
            where TState : struct, IConvertible;

        Task<StoredState<TData, TState>> RetrieveStateData<TData, TState>(string identifier, string machineName, long? version)
            where TData : BaseStateDataModel<TState>
            where TState : struct, IConvertible;
    }

    public class StoredState<TData, TState>
        where TData : BaseStateDataModel<TState>
        where TState : struct, IConvertible
    {
        public StateDto StateMetaData { get; set; }
        public TData Data { get; set; }
    }
}