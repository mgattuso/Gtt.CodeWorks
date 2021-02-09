using System;
using System.Collections.Generic;
using System.Text;
using Stateless;

namespace Gtt.CodeWorks.StateMachines
{
    internal interface IStateMachineDiagramResolver
    {
        (string diagram, string contentType) Diagram();
    }
}
