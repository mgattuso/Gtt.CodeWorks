using System;

namespace Gtt.CodeWorks.StateMachines
{
    public class DefaultStateData<TState, TAction, TData> : IStateData<TState, TAction, TData>
        where TData : BaseStateDataModel<TState>
        where TState : struct, IConvertible
    {
        public TData CurrentState { get; set; }
        public long SerialNumber { get; set; }
        public double CreatedAgo { get; set; }
        public double ModifiedAgo { get; set; }
        public TAction[] Actions { get; set; }
        public TState[] ActiveStates { get; set; }
        public string Machine { get; set; }
        public string InstanceIdentifier { get; set; }
    }
}