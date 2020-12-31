using System;
using System.Collections.Generic;

namespace Gtt.CodeWorks.Clients.HttpClient
{
    public abstract class CodeWorksClientEndpoint
    {
        private readonly Dictionary<string, string> _map;

        protected CodeWorksClientEndpoint(string rootUrl, Dictionary<string, string> urlMap = null)
        {
            if (string.IsNullOrWhiteSpace(rootUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(rootUrl));
            // APPEND TRAILING / IF NEEDED
            if (!rootUrl.EndsWith("/")) rootUrl = rootUrl + "/";

            Root = rootUrl;
            _map = urlMap ?? new Dictionary<string, string>();
        }

        public string Root { get; }
        public Uri GetUrl(string originalUrl)
        {
            string route = _map.GetValueOrDefault(originalUrl) ?? originalUrl ?? "";
            while (route.StartsWith("/"))
            {
                route = route.Substring(1, route.Length - 1);
            }
            var uri = new Uri(route, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                return uri;
            }
            return new Uri(new Uri(Root), uri);
        }
    }
}
