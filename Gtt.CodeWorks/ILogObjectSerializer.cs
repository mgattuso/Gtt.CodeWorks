﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface ILogObjectSerializer
    {
        Task<string> Serialize<T>(T obj);
    }
}
