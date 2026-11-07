using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Demo.Website.Persistence;

public sealed class SqlServerDemoWebsiteDbContext : DemoWebsiteDbContext
{
    public SqlServerDemoWebsiteDbContext(
        DbContextOptions<SqlServerDemoWebsiteDbContext> options)
        : base(options)
    {
    }
}
