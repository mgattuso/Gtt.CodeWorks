using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.StateMachines
{
    public interface IStatefulServiceInstance : IServiceInstance
    {
        (string diagram, string contentType) Diagram();
        Type StateType { get; }
        Type TriggerType { get; }
        Type DataType { get; }
    }
}
