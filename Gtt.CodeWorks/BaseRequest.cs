using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public abstract class BaseRequest : ITraceable
    {
        public Guid CorrelationId { get; set; }
        public Guid? SessionId { get; set; }
        [NeverShownPublicly]
        public int? ServiceHop { get; set; }
    }

    public abstract class BaseIdentifiableRequest<T> : BaseRequest
    {
        public T Identifier { get; }
    }

    public abstract class BaseIdentifiableRequestGuid : BaseIdentifiableRequest<Guid>
    {
    }

    public abstract class BaseIdentifiableRequestInt : BaseIdentifiableRequest<int>
    {
    }

    public abstract class BaseIdentifiableRequestLong : BaseIdentifiableRequest<long>
    {
    }

    public abstract class BaseIdentifiableRequestString : BaseIdentifiableRequest<string>
    {
    }
}
