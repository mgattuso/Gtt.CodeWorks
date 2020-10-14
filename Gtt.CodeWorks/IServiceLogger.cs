﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IServiceLogger
    {
        Task LogRequest<TReq>(Guid correlationId, string serviceName, TReq request) where TReq : new();
        Task LogResponse<TRes>(Guid correlationId, string serviceName, TRes response);
    }
}
