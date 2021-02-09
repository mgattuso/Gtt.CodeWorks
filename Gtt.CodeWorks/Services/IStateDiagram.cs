using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Services
{
    public interface IStateDiagram
    {
        Task<(string diagram, string contentType)> GetDiagram(IServiceInstance serviceInstance);
    }

    public class NullStateDiagram : IStateDiagram
    {
        Task<(string diagram, string contentType)> IStateDiagram.GetDiagram(IServiceInstance serviceInstance)
        {
            return Task.FromResult(("", ""));
        }
    }
}
