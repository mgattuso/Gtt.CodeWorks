using System;
using System.Collections.Generic;

namespace Gtt.CodeWorks.Duplicator
{
    public class CopierSettings
    {
        public bool SkipAllOverrides { get; set; } = true;
        public bool AddAllUsingStatementProcessed { get; set; } = false;
        public List<Type> BaseTypesToRemove { get; } = new List<Type>();
        public Dictionary<Type, Type> ReplacementTypes { get; } = new Dictionary<Type, Type>();
    }
}