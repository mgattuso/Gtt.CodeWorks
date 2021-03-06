using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.StateMachines
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class DerivedIdentifierAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public DerivedIdentifierAttribute()
        {
        }
    }
}
