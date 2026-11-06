using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace HypnoTech.Common.Extensions
{
    public static class UriExtensions
    {
        public static string UpdateQueryParameter(this Uri url, string key, string value)
        {
            var baseUri = url.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
            var query = QueryHelpers.ParseNullableQuery(url.Query);

            if (query == null)
            {
                return $"{baseUri}?{key}={value}";
            }

            var items = query.SelectMany(x => x.Value, (c, v) => new KeyValuePair<string, string>(c.Key, v)).ToList();

            items.RemoveAll(x => x.Key.ToLower() == key.ToLower());

            var qb = new QueryBuilder(items) { { key, value } };
            return baseUri + qb.ToQueryString();
        }
    }
}
