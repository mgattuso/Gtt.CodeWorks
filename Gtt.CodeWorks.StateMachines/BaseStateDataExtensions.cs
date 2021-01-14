using System;
using System.Linq;

namespace Gtt.CodeWorks.StateMachines
{
    public static class BaseStateDataExtensions
    {
        public static bool CanCallAction<TState, TAction, TData>(
            this DefaultStateData<TState, TAction, TData> stateData,
            TAction action)
            where TData : BaseStateDataModel<TState>
            where TState : struct, IConvertible
        {
            if (stateData == null) throw new ArgumentNullException(nameof(stateData));
            if (stateData.Actions == null) return false;
            return stateData.Actions.Contains(action);
        }

        public static bool InFinalState<TState, TAction, TData>(
            this DefaultStateData<TState, TAction, TData> stateData)
            where TData : BaseStateDataModel<TState>
            where TState : struct, IConvertible
        {
            return stateData.Actions != null && stateData.Actions.Length == 0;
        }

        public static bool IsInState<TState, TAction, TData>(
            this DefaultStateData<TState, TAction, TData> stateData, TState state)
            where TData : BaseStateDataModel<TState>
            where TState : struct, IConvertible
        {
            if (stateData == null) throw new ArgumentNullException(nameof(stateData));
            if (stateData.ActiveStates == null) return false;
            return stateData.ActiveStates.Contains(state);
        }
    }
}