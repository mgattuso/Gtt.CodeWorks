using System;
using System.Collections.Generic;

namespace Gtt.CodeWorks.Web
{
    public abstract class CodeWorksClientEndpoint
    {
        private readonly Dictionary<string, string> _map;
        private CodeWorksClientEndpoint(string rootUrl, Dictionary<string, string> urlMap)
        {
            if (string.IsNullOrWhiteSpace(rootUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(rootUrl));
            Root = rootUrl;
            _map = urlMap ?? new Dictionary<string, string>();
        }

        public string Root { get; }
        public Uri GetUrl(string originalUrl)
        {
            string route = _map.GetValueOrDefault(originalUrl) ?? originalUrl;
            var uri = new Uri(route);
            if (uri.IsAbsoluteUri)
            {
                return uri;
            }
            return new Uri(new Uri(Root), uri);
        }
    }
}
