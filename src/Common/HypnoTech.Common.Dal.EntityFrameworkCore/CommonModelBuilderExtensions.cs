using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HypnoTech.Common.Models;
using HypnoTech.Common.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HypnoTech.Common.Dal.EntityFrameworkCore
{
    public static class CommonModelBuilderExtensions
    {
        public static ModelBuilder AddHypnoTechCommonModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.ToTable("adminusers");
                StringId(entity);
                entity.HasIndex(user => user.Username);
                entity.HasIndex(user => user.Email);
                entity.Property(user => user.Roles)
                    .HasConversion(
                        roles => JsonSerializer.Serialize(roles, (JsonSerializerOptions)null),
                        json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions)null))
                    .Metadata.SetValueComparer(StringListComparer());
            });
            modelBuilder.Entity<Config>(entity =>
            {
                entity.ToTable("config");
                entity.Property<int>("PersistenceId").ValueGeneratedOnAdd();
                entity.HasKey("PersistenceId");
                entity.OwnsOne(config => config.Paypal);
                entity.OwnsOne(config => config.Email);
                entity.OwnsOne(config => config.MainSite);
                entity.Navigation(config => config.MainSite).IsRequired();
            });
            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("countries");
                entity.HasKey(country => country.Code);
                entity.Property(country => country.Code).ValueGeneratedNever();
            });
            modelBuilder.Entity<EmailTemplate>(entity =>
            {
                entity.ToTable("emailtemplates");
                StringId(entity);
            });
            modelBuilder.Entity<NavBarLink>(entity =>
            {
                entity.ToTable("navbar");
                StringId(entity);
            });
            modelBuilder.Entity<PasswordReset>(entity =>
            {
                entity.ToTable("passwordresets");
                entity.HasKey(reset => reset.Code);
                entity.Ignore(reset => reset.Expiry);
                entity.Ignore(reset => reset.Valid);
            });
            modelBuilder.Entity<SerilogEntry>(entity =>
            {
                entity.ToTable("serilog");
                StringId(entity);
                entity.Ignore(entry => entry.MessageDisplay);
                entity.Ignore(entry => entry.HasException);
                entity.Ignore(entry => entry.TableClass);
                entity.Ignore(entry => entry.DetailsClass);
                entity.OwnsOne(entry => entry.Properties);
                entity.Navigation(entry => entry.Properties).IsRequired();
            });
            return modelBuilder;
        }

        private static void StringId<TEntity>(
            Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
            where TEntity : class
        {
            entity.HasKey("Id");
            entity.Property<string>("Id").ValueGeneratedNever();
        }

        private static ValueComparer<List<string>> StringListComparer()
        {
            return new ValueComparer<List<string>>(
                (left, right) => left.SequenceEqual(right),
                values => values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value)),
                values => values.ToList());
        }
    }
}
