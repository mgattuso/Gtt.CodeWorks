using System;

namespace Gtt.CodeWorks
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class PrivateDataAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public PrivateDataAttribute()
        {
            
        }
    }
}
