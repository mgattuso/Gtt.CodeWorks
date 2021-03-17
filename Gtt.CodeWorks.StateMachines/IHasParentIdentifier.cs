using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.StateMachines
{
    public interface IHasParentIdentifier
    {
        string ParentIdentifier { get; set; }
    }
}
