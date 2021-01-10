using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class AlwaysPresentAttribute : Attribute
    {
    }
}
