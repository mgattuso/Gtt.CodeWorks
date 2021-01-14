using System;

namespace Gtt.CodeWorks.StateMachines
{
    public abstract class BaseStateDataModel<TState> where TState : struct, IConvertible
    {
        public TState State { get; set; }
    }
}