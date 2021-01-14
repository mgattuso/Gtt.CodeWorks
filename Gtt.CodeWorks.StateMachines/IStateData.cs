using System;

namespace Gtt.CodeWorks.StateMachines
{
    public interface IStateData<TState, TAction, TData>
        where TData : BaseStateDataModel<TState>
        where TState : struct, IConvertible
    {
        TData CurrentState { get; set; }
        long SerialNumber { get; set; }
        double CreatedAgo { get; set; }
        double ModifiedAgo { get; set; }
        TAction[] Actions { get; set; }
        TState[] ActiveStates { get; set; }
        string Machine { get; set; }
        string InstanceIdentifier { get; set; }
    }
}