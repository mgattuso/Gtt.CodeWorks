using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{

    public class HttpDataSerializerOptions
    {
        public EnumSerializationMethod EnumSerializationMethod { get; set; }
        public IncludeDependencyMetaDataStrategy IncludeDependencyMetaData { get; set; }
        public JsonValidationStrategy JsonSchemaValidation { get; set; }
    }

    public enum EnumSerializationMethod
    {
        String = 0,
        Numeric = 1,
        Object = 2
    }

    public enum IncludeDependencyMetaDataStrategy
    {
        None = 0,
        Full = 50
    }

    public enum JsonValidationStrategy
    {
        /// <summary>
        /// Follow the configuration on the server. Additional properties will not cause a validation error
        /// </summary>
        DefaultAllowAdditionalProperties = 0,
        /// <summary>
        /// Follow the configuration on the server. Additional properties will trigger a validation error
        /// </summary>
        DefaultStrict = 1,
        /// <summary>
        /// Force validation for this object. Additional properties will not cause a validation error
        /// </summary>
        ForceAllowAdditionalProperties = 50,
        /// <summary>
        /// Force validation for this object. Additional properties will trigger a validation error
        /// </summary>
        ForceStrict = 51,
        /// <summary>
        /// No validation on the json payload will be performed
        /// </summary>
        None = 100
    }
}
