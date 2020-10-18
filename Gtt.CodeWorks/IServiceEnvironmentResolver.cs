using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public interface IServiceEnvironmentResolver
    {
        CodeWorksEnvironment Environment { get; }
    }

    public class ProductionEnvironmentResolver : IServiceEnvironmentResolver
    {
        public CodeWorksEnvironment Environment => CodeWorksEnvironment.Production;
    }

    public class NonProductionEnvironmentResolver : IServiceEnvironmentResolver
    {
        public CodeWorksEnvironment Environment => CodeWorksEnvironment.NonProduction;
    }
}
