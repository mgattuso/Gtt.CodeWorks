using System;
using System.Linq;

namespace Gtt.CodeWorks.StateMachines
{
    public static class StateMachineExtensions
    {
        public static bool IsInState<TState, TTrigger>(this StateMachineData<TState, TTrigger> stateMachine,
            TState state) where TState : struct, IConvertible where TTrigger : struct, IConvertible
        {
            return stateMachine?.ActiveStates?.Contains(state) ?? false;
        }
    }
}