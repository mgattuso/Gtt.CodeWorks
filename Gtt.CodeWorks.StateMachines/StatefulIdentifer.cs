using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.StateMachines
{
    public class StatefulIdentifier
    {
        public StatefulIdentifier(string identifier, string parentIdentifier)
        {
            ParentIdentifier = parentIdentifier;
            Identifier = identifier;
        }

        public string Identifier { get; }
        public string ParentIdentifier { get; }
        public string Separator => !string.IsNullOrWhiteSpace(ParentIdentifier) ? "-" : "";

        public bool HasParentIdentifier()
        {
            return string.IsNullOrWhiteSpace(ParentIdentifier);
        }

        public string ConsolidatedIdentifier => $"{ParentIdentifier}{Separator}{Identifier}";

        public bool HasValidIdentifiers()
        {
            return !string.IsNullOrWhiteSpace(Identifier);
        }
    }
}
