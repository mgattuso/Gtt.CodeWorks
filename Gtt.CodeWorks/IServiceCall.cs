﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IServiceCall<in TRequest, TResponse> 
        where TRequest : BaseRequest, new()
        where TResponse : new()
    {
        string Name { get; }
        string FullName { get; }
        Task<ServiceResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken);
    }
}