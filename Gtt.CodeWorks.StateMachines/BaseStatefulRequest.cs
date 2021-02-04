using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gtt.CodeWorks.StateMachines
{

    public class BaseStatefulRequest<TTrigger> : BaseRequest where TTrigger : struct, IConvertible
    {
        public TTrigger? Trigger { get; set; }
        [AlwaysPresent]
        public string Identifier { get; set; }
    }

    public class BaseStatefulResponse<TState, TTrigger, TData>
        where TTrigger : struct, IConvertible
        where TState : struct, IConvertible
        where TData : BaseStateDataModel<TState>
    {
        public TData Data { get; set; }
        public StateMachineData<TState, TTrigger> StateMachine { get; set; }
    }

    public class StateMachineData<TState, TTrigger>
        where TTrigger : struct, IConvertible
        where TState : struct, IConvertible
    {
        public string Identifier { get; set; }
        public long SerialNumber { get; set; }
        public TState CurrentState { get; set; }
        public TState[] ActiveStates { get; set; }
        public TTrigger[] AllowedTriggers { get; set; }
    }
}
