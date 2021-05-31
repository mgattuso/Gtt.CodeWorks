using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.StateMachines
{
    public class StatefulIdentifier
    {
        public StatefulIdentifier(string identifier) : this(identifier, string.Empty)
        {
            
        }

        public StatefulIdentifier(string identifier, string parentIdentifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException(nameof(identifier), "Identifier should not be blank, null, or whitespace");
            }
            ParentIdentifier = string.IsNullOrWhiteSpace(parentIdentifier) ? "" : parentIdentifier;
            Identifier = identifier;
        }

        public string Identifier { get; }

        /// <summary>
        /// Returns parentIdentifier. Will never be null. Can be empty string.
        /// </summary>
        public string ParentIdentifier { get; }
        public string Separator => !string.IsNullOrWhiteSpace(ParentIdentifier) ? "-" : "";

        public bool HasParentIdentifier()
        {
            return !string.IsNullOrWhiteSpace(ParentIdentifier);
        }

        public string ConsolidatedIdentifier => $"{ParentIdentifier}{Separator}{Identifier}";

        public bool HasValidIdentifiers()
        {
            return !string.IsNullOrWhiteSpace(Identifier);
        }
    }
}
