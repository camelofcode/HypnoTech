using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Demo.Admin.Persistence;

public sealed class SqlServerDemoAdminDbContext : DemoAdminDbContext
{
    public SqlServerDemoAdminDbContext(
        DbContextOptions<SqlServerDemoAdminDbContext> options)
        : base(options)
    {
    }
}
