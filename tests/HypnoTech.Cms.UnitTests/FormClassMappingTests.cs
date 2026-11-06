using System;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Common.Security;
using Xunit;

namespace HypnoTech.Cms.UnitTests;

public sealed class FormClassMappingTests : IDisposable
{
    public FormClassMappingTests()
    {
        FormClassMapping.Maps.Clear();
    }

    public void Dispose()
    {
        FormClassMapping.Maps.Clear();
    }

    [Fact]
    public void RegisterClassMapping_registers_mapping_for_generic_and_runtime_lookup()
    {
        var mapping = FormClassMapping.RegisterClassMapping<TestArticle>(map =>
        {
            map.SetControllerName("Articles");
            map.SetDisplayName("Article");
        });

        Assert.Same(mapping, FormClassMapping.GetMapping<TestArticle>());
        Assert.Same(mapping, FormClassMapping.GetMapping(typeof(TestArticle)));
        Assert.Equal(typeof(TestArticle), mapping.ClassType);
    }

    [Fact]
    public void RegisterClassMapping_registering_same_type_twice_throws()
    {
        FormClassMapping.RegisterClassMapping<TestArticle>(_ => { });

        Assert.Throws<ArgumentException>(() =>
            FormClassMapping.RegisterClassMapping<TestArticle>(_ => { }));
    }

    [Fact]
    public void Class_configuration_preserves_controller_display_role_data_and_capabilities()
    {
        var mapping = FormClassMapping.RegisterClassMapping<TestArticle>(map =>
        {
            map.SetControllerName("Articles");
            map.SetDisplayName("Article");
            map.RequiresRole("characterisation-editor");
            map.AddDataSource("testArticles");
            map.AddDummyDataSource("test-articles.json");
            map.SetIcon("newspaper");
            map.DisableAdd();
            map.DisableEdit();
            map.DisableRemove();
            map.EnableDuplicate(cloneFiles: true);
            map.WithTitleProperty(article => article.Heading);
            map.CanBeOrdered(article => article.Order);
            map.WithDefaultSort(article => article.Published, ascending: false);
        });

        Assert.Equal("Articles", mapping.ControllerName);
        Assert.Equal("Article", mapping.DisplayName);
        Assert.Equal("characterisation-editor", mapping.RequiredRole);
        Assert.Contains("characterisation-editor", RoleStore.GetAllCmsRoles());
        Assert.Equal("testArticles", mapping.DataSource);
        Assert.Equal("test-articles.json", mapping.DummyDataSource);
        Assert.Equal("newspaper", mapping.Icon);
        Assert.False(mapping.CanAdd);
        Assert.False(mapping.CanEdit);
        Assert.False(mapping.CanRemove);
        Assert.True(mapping.CanDuplicate);
        Assert.True(mapping.CloneFilesWhenDuplicating);
        Assert.Equal(nameof(TestArticle.Heading), mapping.TitleProperty);
        Assert.True(mapping.Ordered);
        Assert.Equal(nameof(TestArticle.Order), mapping.OrderProperty);
        Assert.Equal(nameof(TestArticle.Published), mapping.SortProperty);
        Assert.False(mapping.SortAscending);
    }

    [Fact]
    public void Member_mapping_creates_each_supported_scalar_mapping()
    {
        var mapping = new FormClassMapping<TestArticle>();

        var mappings = new FormMemberMapping[]
        {
            mapping.MapMemberWithoutInput(article => article.Ignored),
            mapping.MapHiddenInputMember(article => article.Hidden),
            mapping.MapTextInputMember(article => article.Heading),
            mapping.MapTextAreaInputMember(article => article.Summary),
            mapping.MapRichTextInputMember(article => article.Body),
            mapping.MapDateInputMember(article => article.Published),
            mapping.MapFileUploadInputMember(article => article.File),
            mapping.MapImageUploadInputMember(article => article.Image),
            mapping.MapSelectListInputMember(article => article.ParentCategoryId),
            mapping.MapStaticSelectListInputMember(article => article.Status),
            mapping.MapNestedSelectListInputMember(article => article.CategoryId, article => article.ChildCategoryId),
            mapping.MapYouTubeInputMember(article => article.VideoId),
            mapping.MapReadOnlyMember(article => article.ReadOnly),
            mapping.MapMoneyInputMember(article => article.Price),
            mapping.MapCheckboxInputMember(article => article.Featured),
            mapping.MapGeneratedMember(article => article.Slug, article => article.Heading.ToLowerInvariant())
        };

        Assert.Collection(
            mappings,
            item => Assert.IsType<NoInputMapping>(item),
            item => Assert.IsType<HiddenInputMapping>(item),
            item => Assert.IsType<TextInputMapping>(item),
            item => Assert.IsType<TextAreaInputMapping>(item),
            item => Assert.IsType<RichTextInputMapping>(item),
            item => Assert.IsType<DateInputMapping>(item),
            item => Assert.IsType<FileUploadInputMapping>(item),
            item => Assert.IsType<ImageUploadInputMapping>(item),
            item => Assert.IsType<SelectListInputMapping>(item),
            item => Assert.IsType<StaticSelectListInputMapping>(item),
            item => Assert.IsType<NestedSelectListInputMapping>(item),
            item => Assert.IsType<YouTubeInputMapping>(item),
            item => Assert.IsType<ReadOnlyMapping>(item),
            item => Assert.IsType<MoneyInputMapping>(item),
            item => Assert.IsType<CheckboxInputMapping<TestArticle>>(item),
            item => Assert.IsType<GeneratedMemberMapping<TestArticle>>(item));
    }

    [Fact]
    public void Repeated_member_mapping_returns_existing_mapping()
    {
        var mapping = new FormClassMapping<TestArticle>();

        var first = mapping.MapTextInputMember(article => article.Heading);
        var second = mapping.MapTextInputMember(article => article.Heading);

        Assert.Same(first, second);
        Assert.Single(mapping.MemberMappings);
    }

    [Fact]
    public void Id_and_title_helpers_use_configured_members()
    {
        var mapping = new FormClassMapping<TestArticle>();
        mapping.MapHiddenInputMember(article => article.Id);
        mapping.WithTitleProperty(article => article.Heading);
        var article = new TestArticle { Heading = "A synthetic article" };

        mapping.SetId(article, "synthetic-id");

        Assert.True(mapping.HasId);
        Assert.Equal("synthetic-id", mapping.GetId(article));
        Assert.Equal("A synthetic article", mapping.GetTitle(article));
    }

    [Fact]
    public void List_and_gallery_mapping_capture_nested_member_configuration()
    {
        var mapping = new FormClassMapping<TestArticle>();

        var list = mapping.MapListInputMember<TestSection, List<TestSection>>(article => article.Sections)
            .MapMembers(section =>
            {
                section.MapId(item => item.Id);
                section.MapTextInputMember(item => item.Name, isNameField: true);
                section.CanBeOrdered(item => item.Order);
            });
        var gallery = mapping.MapPhotoGalleryInputMember<TestPhoto, List<TestPhoto>>(article => article.Photos)
            .MapMembers(photo => photo.Id, photo => photo.Path, photo => photo.Caption)
            .SetImageSize(1200, 800);

        Assert.True(list.Ordered);
        Assert.Equal(nameof(TestSection.Order), list.OrderProperty);
        Assert.Equal(2, list.MemberMappings.Count);
        Assert.Equal(nameof(TestPhoto.Id), gallery.IdName);
        Assert.Equal(nameof(TestPhoto.Path), gallery.ImagePathName);
        Assert.Equal(nameof(TestPhoto.Caption), gallery.CaptionName);
        Assert.Equal(1200, gallery.MaxWidth);
        Assert.Equal(800, gallery.MaxHeight);
    }

    public sealed class TestArticle
    {
        public string Id { get; set; }
        public string Heading { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public DateTime? Published { get; set; }
        public string File { get; set; }
        public string Image { get; set; }
        public string CategoryId { get; set; }
        public string ParentCategoryId { get; set; }
        public string ChildCategoryId { get; set; }
        public string Status { get; set; }
        public string VideoId { get; set; }
        public string ReadOnly { get; set; }
        public decimal Price { get; set; }
        public bool Featured { get; set; }
        public string Slug { get; set; }
        public string Hidden { get; set; }
        public string Ignored { get; set; }
        public int Order { get; set; }
        public List<TestSection> Sections { get; set; } = new();
        public List<TestPhoto> Photos { get; set; } = new();
    }

    public sealed class TestSection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }

    public sealed class TestPhoto
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string Caption { get; set; }
    }
}
