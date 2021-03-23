using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.StateMachines
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class InternalTriggerAttribute : Attribute
    {
    }
}
