using HypnoTech.Common.Extensions;
using Xunit;

namespace HypnoTech.Common.UnitTests;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("A Simple Title", "a-simple-title")]
    [InlineData("Crème brûlée", "creme-brulee")]
    [InlineData("Repeated  --  separators", "repeated-separators")]
    [InlineData("Symbols!? are removed", "symbols-are-removed")]
    public void GenerateSlug_normalises_existing_behaviour(string input, string expected)
    {
        Assert.Equal(expected, input.GenerateSlug());
    }

    [Fact]
    public void GenerateSlug_truncates_before_replacing_spaces()
    {
        const string input = "one two three four five six seven eight nine ten eleven twelve thirteen fourteen";

        var slug = input.GenerateSlug();

        Assert.True(slug.Length <= 60);
        Assert.Equal("one-two-three-four-five-six-seven-eight-nine-ten-eleven-twel", slug);
    }

    [Theory]
    [InlineData("first\r\nsecond", "first<br />second")]
    [InlineData("first\rsecond", "first<br />second")]
    [InlineData("first\nsecond", "first<br />second")]
    public void LineBreaksToHtml_supports_existing_newline_forms(string input, string expected)
    {
        Assert.Equal(expected, input.LineBreaksToHtml());
    }
}
