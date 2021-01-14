using System;

namespace Gtt.CodeWorks.StateMachines
{
    public abstract class BaseReferenceModel
    {
        protected BaseReferenceModel(string identifier, Guid correlationId)
        {
            CorrelationId = correlationId;
            Identifier = identifier;
        }

        protected BaseReferenceModel(Guid identifier, Guid correlationId)
        {
            CorrelationId = correlationId;
            Identifier = identifier.ToString();
        }

        public string Identifier { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
