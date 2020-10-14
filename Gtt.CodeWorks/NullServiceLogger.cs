using System;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public class NullServiceLogger : IServiceLogger
    {
        private NullServiceLogger()
        {
            
        }

        public Task LogRequest<TReq>(Guid correlationId, string serviceName, TReq request) where TReq : new()
        {
            return Task.CompletedTask;
        }

        public Task LogResponse<TRes>(Guid correlationId, string serviceName, TRes response)
        {
            return Task.CompletedTask;
        }

        public static NullServiceLogger Instance => new NullServiceLogger();
    }
}