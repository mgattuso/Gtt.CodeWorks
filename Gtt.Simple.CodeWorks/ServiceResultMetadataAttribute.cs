using System;

namespace Gtt.Simple.CodeWorks
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ServiceResultMetadataAttribute : Attribute
    {
        public ResultOutcome Outcome { get; }
        public ResultCategory Category { get; }
        public int HttpStatusCode { get; }

        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public ServiceResultMetadataAttribute(ResultOutcome success, ResultCategory category, int httpStatusCode)
        {
            Outcome = success;
            Category = category;
            HttpStatusCode = httpStatusCode;
            if (success == ResultOutcome.Successful && category != ResultCategory.Successful)
            {
                throw new ArgumentException("If successful then the category must also be successful", nameof(category));
            }
        }
    }
}
