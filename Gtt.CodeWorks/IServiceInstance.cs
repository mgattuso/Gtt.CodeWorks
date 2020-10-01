using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IServiceInstance<in TRequest, TResponse> where TRequest : new() where TResponse : new()
    {
        Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken);
        string Name { get; }
    }
}
