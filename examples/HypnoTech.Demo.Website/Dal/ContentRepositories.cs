using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Demo.Models;

namespace HypnoTech.Demo.Website.Dal;

public interface IArticleRepository
{
    Task<ICollection<Article>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Article> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
public interface IArtistRepository
{
    Task<ICollection<Artist>> GetAllAsync(CancellationToken cancellationToken = default);
}
public interface IEventRepository
{
    Task<ICollection<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Event> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
