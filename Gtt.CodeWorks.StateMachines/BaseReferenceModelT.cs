using System;

namespace Gtt.CodeWorks.StateMachines
{
    public abstract class BaseReferenceModel<T> : BaseReferenceModel
    {
        protected BaseReferenceModel(T identifier, Guid correlationId) : base(identifier.ToString(), correlationId)
        {
            Identifier = identifier;
        }

        public new T Identifier { get; set; }
    }
}