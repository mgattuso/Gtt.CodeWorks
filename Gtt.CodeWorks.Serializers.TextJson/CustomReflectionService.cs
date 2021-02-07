using Namotion.Reflection;
using Newtonsoft.Json;
using NJsonSchema.Generation;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class CustomReflectionService : DefaultReflectionService
    {
        public override JsonTypeDescription GetDescription(ContextualType contextualType,
            ReferenceTypeNullHandling defaultReferenceTypeNullHandling, JsonSchemaGeneratorSettings settings)
        {
            return base.GetDescription(contextualType, defaultReferenceTypeNullHandling, settings);
        }

        public override bool IsNullable(ContextualType contextualType, ReferenceTypeNullHandling defaultReferenceTypeNullHandling)
        {
            if (contextualType.GetContextAttribute<AlwaysPresentAttribute>() != null) return false;
            return base.IsNullable(contextualType, defaultReferenceTypeNullHandling);
        }

        public override bool IsStringEnum(ContextualType contextualType, JsonSerializerSettings serializerSettings)
        {
            return base.IsStringEnum(contextualType, serializerSettings);
        }
    }
}