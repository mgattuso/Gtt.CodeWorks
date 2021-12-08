using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.Simple.CodeWorks
{
    public static class ServiceResultExpander
    {
        private static readonly IDictionary<ServiceResult, Tuple<ResultOutcome, ResultCategory, int>> Cache =
            new Dictionary<ServiceResult, Tuple<ResultOutcome, ResultCategory, int>>();
        static ServiceResultExpander()
        {
            // CACHE THE RESULTS
            var arr = Enum.GetValues(typeof(ServiceResult));
            foreach (var a in arr)
            {
                var v = (ServiceResult)a;
                var attr = v.GetAttribute<ServiceResultMetadataAttribute>();
                if (attr == null)
                {
                    throw new Exception($"ServiceResult value {a} does not have the attribute {nameof(ServiceResultMetadataAttribute)} defined");
                }

                Cache[v] = Tuple.Create(attr.Outcome, attr.Category, attr.HttpStatusCode);
            }
        }

        private static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            // https://stackoverflow.com/a/19621488/185045
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0
                ? (T)attributes[0]
                : null;
        }

        public static ResultOutcome Outcome(this ServiceResult result)
        {
            return Cache[result].Item1;
        }

        public static ResultCategory Category(this ServiceResult result)
        {
            return Cache[result].Item2;
        }

        public static int HttpStatusCode(this ServiceResult result)
        {
            return Cache[result].Item3;
        }
    }
}
