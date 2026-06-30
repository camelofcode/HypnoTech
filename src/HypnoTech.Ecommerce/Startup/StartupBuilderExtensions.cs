using System.Collections.Generic;
using HypnoTech.Cms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HypnoTech.Common.Emails;
using HypnoTech.Common.Extensions;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Startup
{
    public static class StartupBuilderExtensions
    {
        public static void AddEcommerceAdmin(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterEcommerceModels(configuration);

            services.UseEmails();
            services.Configure<ECommerceOptions>(configuration.GetSection("ecommerce"));
        }

        private static void RegisterEcommerceModels(IConfiguration configuration)
        {
            if (configuration.GetValue("ecommerce:enabled", false))
            {
                FormClassMapping.RegisterClassMapping<MerchCategoryType>(cm =>
                {
                    cm.SetDisplayName("Merch Types");
                    cm.SetControllerName("MerchTypes");
                    cm.RequiresRole("merch");
                    cm.AddDummyDataSource("merchcategorytypes");
                    cm.AddDataSource("merchcategorytypes");
                    cm.MapHiddenInputMember(mm => mm.Id);
                    cm.MapTextInputMember(mm => mm.Name).IncludeInTable(0).Required();
                    cm.CanBeOrdered(mm => mm.Order);

                    cm.MapListInputMember<MerchSubcategoryType, List<MerchSubcategoryType>>(mm => mm.SubcategoryTypes)
                        .MapMembers(p =>
                        {
                            p.MapTextInputMember(mm => mm.Name, true).Required();
                        }).SetSingularObjectName("Subcategory").CanBeOrdered(mm => mm.Order).Required(true);

                    cm.SetIcon("fa fa-coins");
                });


                FormClassMapping.RegisterClassMapping<MerchItem>(cm =>
                {
                    cm.SetDisplayName("Merch");
                    cm.SetControllerName("Merch");
                    cm.RequiresRole("merch");
                    cm.AddDummyDataSource("merches");
                    cm.AddDataSource("merches");

                    cm.MapHiddenInputMember(mm => mm.Id);
                    cm.MapTextInputMember(mm => mm.Name).IncludeInTable(0).Searchable().Required();
                    cm.MapTextInputMember(mm => mm.Description).Required();
                    cm.MapMoneyInputMember(mm => mm.Price).Required();
                    cm.MapImageUploadInputMember(mm => mm.ImagePath).SetImageSize(500, 500).SetTargetDirectory("Merch").WithDisplayText("Image").Required();

                    cm.MapNestedSelectListInputMember(m1 => m1.Category, m2 => m2.SubCategory)
                        .SetListOptions<MerchCategoryType, string, string, List<MerchSubcategoryType>, MerchSubcategoryType, string, string>(p1 => p1.Name, p2 => p2.Name, p3 => p3.SubcategoryTypes, p4 => p4.Name, p5 => p5.Name, "Subcategory")
                        .WithDisplayText("Category").IncludeInTable(1).Searchable().Required();

                    cm.MapMemberWithoutInput(mm => mm.SubCategory).IncludeInTable("Sub Category", 2).Searchable();

                    cm.MapSelectListInputMember(mm => mm.PostageProfile)
                        .SetListOptions<PostageProfile, string, string>(p => p.Name, p => p.Name)
                        .WithDisplayText("Postage Profile").Required();

                    cm.MapSelectListInputMember(mm => mm.DownloadCodeId)
                        .SetListOptions<DownloadCodeCollection, string, string>(p => p.Id, p => p.Name, optional: true)
                        .WithDisplayText("Download Code");

                    cm.MapListInputMember<MerchOption, List<MerchOption>>(mm => mm.Options).MapMembers(p =>
                    {
                        p.MapId(mm => mm.Id);
                        p.MapTextInputMember(mm => mm.Name, true).Required();
                    }).SetSingularObjectName("Option");

                    cm.MapGeneratedMember(mm => mm.Slug, merch => merch.Name.GenerateSlug());

                    cm.SetIcon("fa fa-coins");
                });
            }
        }
    }
}
