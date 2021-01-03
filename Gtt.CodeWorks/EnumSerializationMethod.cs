using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{

    public class HttpDataSerializerOptions
    {
        public EnumSerializationMethod EnumSerializationMethod { get; set; }
        public bool IncludeDependencyMetaData { get; set; }
    }

    public enum EnumSerializationMethod
    {
        String = 0,
        Numeric = 1,
        Object = 2
    }
}
