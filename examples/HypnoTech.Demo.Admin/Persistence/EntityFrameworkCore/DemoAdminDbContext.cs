using HypnoTech.Cms.EntityFrameworkCore;
using HypnoTech.Common.Dal.EntityFrameworkCore;
using HypnoTech.Ecommerce.Dal.EntityFrameworkCore;
using HypnoTech.Media.Dal.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Demo.Admin.Persistence;

public abstract class DemoAdminDbContext : DbContext
{
    protected DemoAdminDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddHypnoTechCommonModel();
        modelBuilder.AddHypnoTechEcommerceModel();
        modelBuilder.AddHypnoTechMediaModel();
        modelBuilder.AddHypnoTechCmsModel();
    }

}
