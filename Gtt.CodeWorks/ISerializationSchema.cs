using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface ISerializationSchema
    {
        Task<IDictionary<string, string[]>> ValidateSchema(byte[] contents, Type type, HttpDataSerializerOptions options = null);
        Task<string> SerializeErrorReport(IEnumerable<ErrorCodeData> errors);
        Task<string> SerializeExample(Type t, HttpDataSerializerOptions options = null);
        Task<string> SerializeSchema(Type t, HttpDataSerializerOptions options = null);
    }
}