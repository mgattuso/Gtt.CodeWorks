using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.EasyNetQ
{
    public enum DeleteMode
    {
        EverythingExceptTypeExchange,
        OnlyHoldingQueues,
        Everything
    }
}
