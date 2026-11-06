using System;
using HypnoTech.Common.Extensions;
using Xunit;

namespace HypnoTech.Common.UnitTests;

public sealed class UriExtensionsTests
{
    [Fact]
    public void UpdateQueryParameter_adds_value_when_query_is_empty()
    {
        var result = new Uri("https://example.test/articles").UpdateQueryParameter("page", "2");

        Assert.Equal("articles?page=2", result);
    }

    [Fact]
    public void UpdateQueryParameter_replaces_key_case_insensitively_and_preserves_other_values()
    {
        var result = new Uri("https://example.test/articles?Page=1&filter=news")
            .UpdateQueryParameter("page", "2");

        Assert.Equal("articles?filter=news&page=2", result);
    }
}
