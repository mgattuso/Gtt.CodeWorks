using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gtt.CodeWorks.Services;

namespace Gtt.CodeWorks.StateMachines
{
    public class StatelessStateDiagram : IStateDiagram
    {
        public Task<(string diagram, string contentType)> GetDiagram(IServiceInstance serviceInstance)
        {
            var ssi = serviceInstance as IStatefulServiceInstance;
            if (ssi == null)
            {
                return Task.FromResult(("",""));
            }
            var d= ssi.Diagram();
            return Task.FromResult(d);
        }

    }
}
