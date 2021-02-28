using System;

namespace Gtt.CodeWorks.StateMachines
{
    public class BaseStatefulResponse<TState, TTrigger, TData>
        where TTrigger : struct, IConvertible
        where TState : struct, IConvertible
        where TData : BaseStateDataModel<TState>, new()
    {
        public TData Model { get; set; } = new TData();
        public StateMachineData<TState, TTrigger> StateMachine { get; set; }
    }
}