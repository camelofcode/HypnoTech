using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HypnoTech.Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HypnoTech.Ecommerce.Dal.EntityFrameworkCore
{
    public static class EcommerceModelBuilderExtensions
    {
        public static ModelBuilder AddHypnoTechEcommerceModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MerchCategoryType>(entity =>
            {
                entity.ToTable("merchcategorytypes");
                StringId(entity);
                var subcategories = entity.OwnsMany(category => category.SubcategoryTypes);
                subcategories.ToTable("merchcategorytypes_SubcategoryTypes");
                subcategories.Property<int>("PersistenceId").ValueGeneratedOnAdd();
                subcategories.HasKey("PersistenceId");
            });
            modelBuilder.Entity<MerchItem>(entity =>
            {
                entity.ToTable("merches");
                StringId(entity);
                entity.Ignore(item => item.FullUrl);
                var options = entity.OwnsMany(item => item.Options);
                options.ToTable("merches_Options");
                options.Property(option => option.Id).ValueGeneratedNever();
                options.HasKey("MerchItemId", nameof(MerchOption.Id));
            });
            modelBuilder.Entity<PostageProfile>(entity =>
            {
                entity.ToTable("postageprofiles");
                StringId(entity);
                entity.Ignore(profile => profile.CanDelete);
                entity.OwnsOne(profile => profile.UnitedKingdom);
                entity.OwnsOne(profile => profile.EuropeanUnion);
                entity.OwnsOne(profile => profile.RestOfWorld1);
                entity.OwnsOne(profile => profile.RestOfWorld2);
                entity.OwnsOne(profile => profile.RestOfWorld3);
                entity.Navigation(profile => profile.UnitedKingdom).IsRequired();
                entity.Navigation(profile => profile.EuropeanUnion).IsRequired();
                entity.Navigation(profile => profile.RestOfWorld1).IsRequired();
                entity.Navigation(profile => profile.RestOfWorld2).IsRequired();
                entity.Navigation(profile => profile.RestOfWorld3).IsRequired();
                ConfigurePostageRegion(entity.OwnsOne(profile => profile.UnitedKingdom));
                ConfigurePostageRegion(entity.OwnsOne(profile => profile.EuropeanUnion));
                ConfigurePostageRegion(entity.OwnsOne(profile => profile.RestOfWorld1));
                ConfigurePostageRegion(entity.OwnsOne(profile => profile.RestOfWorld2));
                ConfigurePostageRegion(entity.OwnsOne(profile => profile.RestOfWorld3));
            });
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(order => order.PaymentId);
                entity.Property(order => order.PaymentId).ValueGeneratedNever();
                entity.Ignore(order => order.SpecialDeliveryCost);
                entity.Ignore(order => order.PostageDisplay);
                entity.Ignore(order => order.TotalDisplay);
                entity.Ignore(order => order.SubTotalDisplay);
                entity.Property(order => order.Postage).HasPrecision(18, 2);
                entity.Property(order => order.Total).HasPrecision(18, 2);
                entity.Property(order => order.SubTotal).HasPrecision(18, 2);
                entity.OwnsOne(order => order.Address);
                entity.Navigation(order => order.Address).IsRequired();
                var items = entity.OwnsMany(order => order.Items);
                items.ToTable("orders_Items");
                items.Property<int>("PersistenceId").ValueGeneratedOnAdd();
                items.Property(item => item.RowPrice).HasPrecision(18, 2);
                items.HasKey("PersistenceId");
                entity.Property(order => order.DownloadCodeIds)
                    .HasConversion(
                        values => JsonSerializer.Serialize(values, (JsonSerializerOptions)null),
                        json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions)null))
                    .Metadata.SetValueComparer(StringListComparer());
            });
            modelBuilder.Entity<DownloadCodeCollection>(entity =>
            {
                entity.ToTable("downloadcodes");
                StringId(entity);
                entity.Ignore(collection => collection.TotalCodes);
                entity.Ignore(collection => collection.AssignedCodes);
                entity.Ignore(collection => collection.UnusedCodes);
                var codes = entity.OwnsMany(collection => collection.Codes);
                codes.ToTable("downloadcodes_Codes");
                codes.HasKey("DownloadCodeCollectionId", nameof(DownloadCode.Code));
                codes.Ignore(code => code.Assigned);
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

        private static void ConfigurePostageRegion(
            Microsoft.EntityFrameworkCore.Metadata.Builders.OwnedNavigationBuilder<
                PostageProfile,
                PostageRegion> region)
        {
            region.Property(value => value.Alone).HasPrecision(18, 2);
            region.Property(value => value.WithOthers).HasPrecision(18, 2);
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
