using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IObjectSerializer
    {
        Task<string> Serialize(Type t, Object obj);
        Task<string> Serialize<T>(T obj);
        Task<T> Deserialize<T>(string str);
        Task<object> Deserialize(Type t, string str);
    }
}
