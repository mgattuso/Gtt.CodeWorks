using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.StateMachines
{
    public class StatefulIdentifier
    {
        public StatefulIdentifier(string parentIdentifier, string identifier)
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
    }
}
