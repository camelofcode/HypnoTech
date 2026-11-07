using System;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HypnoTech.Cms.EntityFrameworkCore
{
    public static class CmsModelBuilderExtensions
    {
        public static ModelBuilder AddHypnoTechCmsModel(this ModelBuilder modelBuilder)
        {
            foreach (var mapping in FormClassMapping.Maps.Values)
            {
                ConfigureCmsEntity(modelBuilder, mapping);
            }

            foreach (var registration in ModelRegistry.Registrations)
            {
                if (modelBuilder.Model.FindEntityType(registration.Type) == null)
                {
                    ConfigureRegisteredEntity(modelBuilder, registration.Type, registration.DataName);
                }
            }

            return modelBuilder;
        }

        private static void ConfigureCmsEntity(ModelBuilder modelBuilder, FormClassMapping mapping)
        {
            var entity = modelBuilder.Entity(mapping.ClassType);
            entity.ToTable(mapping.DataSource);
            ConfigureStringId(entity, mapping.ClassType);

            foreach (var member in mapping.MemberMappings.Where(member =>
                         member is ListInputMapping || member is PhotoGalleryInputMapping))
            {
                ConfigureOwnedCollection(entity, mapping, member);
            }

            foreach (var child in mapping.Children)
            {
                var childEntity = modelBuilder.Entity(child.Type);
                childEntity.ToTable(child.DataStore);
                ConfigureStringId(childEntity, child.Type);

                entity.HasOne(child.Type, child.TypeInfo.Name)
                    .WithMany()
                    .HasForeignKey(child.IdInfo.Name)
                    .OnDelete(DeleteBehavior.NoAction);
            }
        }

        private static void ConfigureRegisteredEntity(
            ModelBuilder modelBuilder,
            Type type,
            string tableName)
        {
            var entity = modelBuilder.Entity(type);
            entity.ToTable(tableName);

            var id = type.GetProperty("Id");
            if (id?.PropertyType == typeof(string))
            {
                ConfigureStringId(entity, type);
                return;
            }

            var code = type.GetProperty("Code");
            if (code != null)
            {
                entity.HasKey(code.Name);
                return;
            }

            entity.HasNoKey();
        }

        private static void ConfigureStringId(EntityTypeBuilder entity, Type type)
        {
            var id = type.GetProperty("Id") ??
                     throw new InvalidOperationException(
                         $"Entity Framework Core CMS type '{type.FullName}' must expose a public Id property.");
            if (id.PropertyType != typeof(string))
            {
                throw new InvalidOperationException(
                    $"Entity Framework Core CMS type '{type.FullName}' must use a string Id property.");
            }

            entity.HasKey(id.Name);
            entity.Property(id.Name).ValueGeneratedNever();
        }

        private static void ConfigureOwnedCollection(
            EntityTypeBuilder entity,
            FormClassMapping mapping,
            FormMemberMapping member)
        {
            var itemType = GetCollectionItemType(member.MemberType) ??
                           throw new InvalidOperationException(
                               $"Mapped CMS collection '{mapping.ClassType.FullName}.{member.MemberName}' " +
                               "must expose a generic item type.");
            var owned = entity.OwnsMany(itemType, member.MemberName);
            var ownerKey = $"{mapping.ClassType.Name}Id";
            owned.ToTable($"{mapping.DataSource}_{member.MemberName}");
            owned.WithOwner().HasForeignKey(ownerKey);
            owned.Property<int>("PersistenceOrder").ValueGeneratedNever();

            var id = itemType.GetProperty("Id");
            if (id?.PropertyType == typeof(string))
            {
                owned.Property(id.Name).ValueGeneratedNever();
                owned.HasKey(ownerKey, id.Name);
            }
            else
            {
                owned.Property<int>("PersistenceId").ValueGeneratedOnAdd();
                owned.HasKey("PersistenceId");
            }
        }

        private static Type GetCollectionItemType(Type collectionType)
        {
            if (collectionType.IsArray)
            {
                return collectionType.GetElementType();
            }

            if (collectionType.IsGenericType)
            {
                return collectionType.GetGenericArguments().FirstOrDefault();
            }

            return collectionType.GetInterfaces()
                .FirstOrDefault(type =>
                    type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                ?.GetGenericArguments()[0];
        }
    }
}
