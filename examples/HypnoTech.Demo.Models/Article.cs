using System;

namespace HypnoTech.Demo.Models;

public sealed class Article
{
    public string Id { get; set; }

    public string Title { get; set; }

    public string Summary { get; set; }

    public string Body { get; set; }

    public DateTime PublishedAt { get; set; }

    public string HeroImagePath { get; set; }

    public bool Featured { get; set; }

    public string Slug { get; set; }
}
