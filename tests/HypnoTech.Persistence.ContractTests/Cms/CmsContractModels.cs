using System;
using System.Collections.Generic;

namespace HypnoTech.Persistence.ContractTests.Cms;

public sealed class CmsContractArticle
{
    public string Id { get; set; }

    public string Heading { get; set; }

    public string Summary { get; set; }

    public DateTime? Published { get; set; }

    public List<CmsContractSection> Sections { get; set; } = new();

    public List<CmsContractPhoto> Photos { get; set; } = new();

    public string ChildId { get; set; }

    public CmsContractChild Child { get; set; }
}

public sealed class CmsContractSection
{
    public string Id { get; set; }

    public string Name { get; set; }
}

public sealed class CmsContractPhoto
{
    public string Id { get; set; }

    public string Path { get; set; }

    public string Caption { get; set; }
}

public sealed class CmsContractOrderedDocument
{
    public string Id { get; set; }

    public string Name { get; set; }

    public int Order { get; set; }
}

public sealed class CmsContractNonSearchableDocument
{
    public string Id { get; set; }

    public string Name { get; set; }
}

public sealed class CmsContractChild
{
    public string Id { get; set; }

    public string Name { get; set; }
}

public sealed class CmsContractChoice
{
    public string Id { get; set; }

    public string Name { get; set; }
}
