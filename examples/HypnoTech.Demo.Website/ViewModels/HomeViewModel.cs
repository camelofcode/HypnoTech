using System.Collections.Generic;
using HypnoTech.Demo.Models;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Demo.Website.ViewModels;

public sealed class HomeViewModel
{
    public IReadOnlyCollection<Article> Articles { get; init; }

    public IReadOnlyCollection<Event> Events { get; init; }

    public IReadOnlyCollection<Artist> Artists { get; init; }

    public IReadOnlyCollection<MerchItem> ShopItems { get; init; }
}
