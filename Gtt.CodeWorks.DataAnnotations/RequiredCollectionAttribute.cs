using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Gtt.CodeWorks.DataAnnotations
{
    /// <summary>
    /// Ensures a collection is present and populated with at least one or the specified number of entries
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public sealed class RequiredCollectionAttribute : ValidationAttribute
    {
        private readonly int _minimumElements;
        private readonly int _maximumElements;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumElements">The minimum number of elements expected in the collection. Defaults to <value>1</value>.
        /// Setting to 0 would ensure the collection is present in the object but is not required to be populated</param>
        /// <param name="maximumElements">The maximum number of elements expected in the collection.</param>
        public RequiredCollectionAttribute(int minimumElements = 1, int maximumElements = Int32.MaxValue) : base(CreateErrorMessage(minimumElements, maximumElements))
        {
            _minimumElements = minimumElements;
            _maximumElements = maximumElements;
        }

        public override bool IsValid(object value)
        {
            if (!(value is ICollection coll)) return false;
            return coll.Count >= _minimumElements && coll.Count <= _maximumElements;
        }

        private static string CreateErrorMessage(int min, int max)
        {
            if (min <= 0 && max == int.MaxValue)
                return "collection must not be null";

            if (min == 0 && max < int.MaxValue)
                return $"collection must have {max} or less values";

            return $"collection must have between {min} and {max} values";
        }
    }
}
