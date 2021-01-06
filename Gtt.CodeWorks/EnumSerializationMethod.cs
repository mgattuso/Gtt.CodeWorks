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
        /// Follow the server configuration. Typically this is validation in non-prod.
        /// No validation in production environments
        /// </summary>
        Default = 0,
        // No JSON schema validation
        None = 10,
        /// <summary>
        /// Force JSON schema validation
        /// </summary>
        ForceOverride = 20
    }
}
