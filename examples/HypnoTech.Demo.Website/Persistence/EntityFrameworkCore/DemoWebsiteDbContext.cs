using HypnoTech.Common.Dal.EntityFrameworkCore;
using HypnoTech.Demo.Models;
using HypnoTech.Ecommerce.Dal.EntityFrameworkCore;
using HypnoTech.Media.Dal.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Demo.Website.Persistence;

public abstract class DemoWebsiteDbContext : DbContext
{
    protected DemoWebsiteDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddHypnoTechCommonModel();
        modelBuilder.AddHypnoTechEcommerceModel();
        modelBuilder.AddHypnoTechMediaModel();

        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("demoArticles");
            entity.HasKey(article => article.Id);
            entity.Property(article => article.Id).ValueGeneratedNever();
        });
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.ToTable("demoArtists");
            entity.HasKey(artist => artist.Id);
            entity.Property(artist => artist.Id).ValueGeneratedNever();
        });
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("demoEvents");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).ValueGeneratedNever();
            var schedule = entity.OwnsMany(item => item.Schedule);
            schedule.ToTable("demoEvents_Schedule");
            schedule.WithOwner().HasForeignKey("EventId");
            schedule.Property(entry => entry.Id).ValueGeneratedNever();
            schedule.Property<int>("PersistenceOrder").ValueGeneratedNever();
            schedule.HasKey("EventId", nameof(ScheduleItem.Id));
        });

    }
}
