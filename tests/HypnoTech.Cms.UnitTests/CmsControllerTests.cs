using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Dtos.Response;
using HypnoTech.Cms.Forms;
using HypnoTech.Cms.Mvc.Controllers;
using HypnoTech.Cms.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Xunit;

namespace HypnoTech.Cms.UnitTests;

public sealed class CmsControllerTests : IDisposable
{
    private readonly StubCmsService _service = new();
    private readonly RecordingFlashMessage _flash = new();

    public CmsControllerTests()
    {
        FormClassMapping.Maps.Clear();
        RegisterMapping();
    }

    public void Dispose()
    {
        FormClassMapping.Maps.Clear();
    }

    [Fact]
    public void Add_post_calls_service_flashes_confirmation_and_redirects_to_mapping_index()
    {
        var controller = CreateController();
        var model = new ControllerDocument { Title = "Synthetic page" };

        var result = controller.Add(model, [], []);

        Assert.Same(model, _service.Added);
        Assert.Equal("Added 'Synthetic page'", _flash.LastConfirmation);
        Assert.Equal("/Cms/SyntheticPages", Assert.IsType<RedirectResult>(result).Url);
    }

    [Fact]
    public void Add_post_returns_form_without_writing_when_validation_fails()
    {
        _service.IsValid = false;
        var controller = CreateController();

        var result = controller.Add(new ControllerDocument { Title = "Invalid" }, [], []);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Cms/Add", view.ViewName);
        Assert.Same(_service.ValidationForm, view.Model);
        Assert.Null(_service.Added);
        Assert.Null(_flash.LastConfirmation);
    }

    [Fact]
    public void Edit_duplicate_remove_and_order_actions_delegate_and_preserve_redirects()
    {
        var existing = new ControllerDocument { Id = "existing", Title = "Existing page" };
        _service.ById = existing;
        var controller = CreateController();

        var edit = controller.Edit(existing, [], [], []);
        var duplicate = controller.Duplicate(existing, [], []);
        var remove = controller.Remove(existing.Id);
        var moveUp = controller.MoveUp(existing.Id);
        var moveDown = controller.MoveDown(existing.Id);

        Assert.Same(existing, _service.Edited);
        Assert.Same(existing, _service.Duplicated);
        Assert.Equal(existing.Id, _service.RemovedId);
        Assert.Equal(existing.Id, _service.MovedUpId);
        Assert.Equal(existing.Id, _service.MovedDownId);
        Assert.All(new IActionResult[] { edit, duplicate, remove, moveUp, moveDown }, result =>
            Assert.Equal("/Cms/SyntheticPages", Assert.IsType<RedirectResult>(result).Url));
        Assert.Equal("Removed 'Existing page'", _flash.LastConfirmation);
    }

    [Theory]
    [InlineData(nameof(FormClassMapping.DisableAdd), nameof(CmsController<ControllerDocument>.Add))]
    [InlineData(nameof(FormClassMapping.DisableEdit), nameof(CmsController<ControllerDocument>.Edit))]
    [InlineData(nameof(FormClassMapping.DisableRemove), nameof(CmsController<ControllerDocument>.Remove))]
    public void Disabled_capability_throws_not_supported(string disableMethod, string action)
    {
        var mapping = FormClassMapping.GetMapping<ControllerDocument>();
        typeof(FormClassMapping).GetMethod(disableMethod)?.Invoke(mapping, null);
        _service.ById = new ControllerDocument { Id = "existing", Title = "Existing" };
        var controller = CreateController();

        Assert.Throws<NotSupportedException>(() => action switch
        {
            nameof(CmsController<ControllerDocument>.Add) => controller.Add(),
            nameof(CmsController<ControllerDocument>.Edit) => controller.Edit("existing"),
            nameof(CmsController<ControllerDocument>.Remove) => controller.Remove("existing"),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        });
    }

    [Fact]
    public void Duplicate_and_ordering_require_explicit_mapping_capabilities()
    {
        FormClassMapping.Maps.Clear();
        FormClassMapping.RegisterClassMapping<ControllerDocument>(map =>
        {
            map.SetControllerName("SyntheticPages");
            map.SetDisplayName("Synthetic Page");
            map.EnableDuplicate();
        });
        var controller = CreateController();

        Assert.Throws<NotSupportedException>(() => controller.MoveUp("id"));
        Assert.Throws<NotSupportedException>(() => controller.MoveDown("id"));

        FormClassMapping.Maps.Clear();
        FormClassMapping.RegisterClassMapping<ControllerDocument>(map =>
        {
            map.SetControllerName("SyntheticPages");
            map.SetDisplayName("Synthetic Page");
        });
        controller = CreateController();
        Assert.Throws<NotSupportedException>(() => controller.Duplicate("id"));
    }

    [Fact]
    public void Mutating_actions_have_expected_http_method_and_antiforgery_attributes()
    {
        var methods = typeof(CmsController<ControllerDocument>).GetMethods(BindingFlags.Instance | BindingFlags.Public);

        AssertActionAttributes(methods, nameof(CmsController<ControllerDocument>.Add), typeof(HttpPostAttribute), typeof(ValidateAntiForgeryTokenAttribute));
        AssertActionAttributes(methods, nameof(CmsController<ControllerDocument>.Edit), typeof(HttpPostAttribute), typeof(ValidateAntiForgeryTokenAttribute));
        AssertActionAttributes(methods, nameof(CmsController<ControllerDocument>.Duplicate), typeof(HttpPostAttribute), typeof(ValidateAntiForgeryTokenAttribute));
        AssertActionAttributes(methods, nameof(CmsController<ControllerDocument>.Remove), typeof(HttpPostAttribute), typeof(ValidateAntiForgeryTokenAttribute));
    }

    private static void AssertActionAttributes(IEnumerable<MethodInfo> methods, string name, params Type[] attributes)
    {
        var method = methods.Single(candidate =>
            candidate.Name == name && candidate.GetCustomAttribute<HttpPostAttribute>() != null);

        foreach (var attribute in attributes)
        {
            Assert.NotNull(method.GetCustomAttribute(attribute));
        }
    }

    private CmsController<ControllerDocument> CreateController()
    {
        return new CmsController<ControllerDocument>(_service, _flash)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static void RegisterMapping()
    {
        FormClassMapping.RegisterClassMapping<ControllerDocument>(mapping =>
        {
            mapping.SetControllerName("SyntheticPages");
            mapping.SetDisplayName("Synthetic Page");
            mapping.MapHiddenInputMember(document => document.Id);
            mapping.MapTextInputMember(document => document.Title);
            mapping.EnableDuplicate();
            mapping.CanBeOrdered(document => document.Order);
        });
    }

    public sealed class ControllerDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
    }

    private sealed class StubCmsService : ICmsService<ControllerDocument>
    {
        public bool IsValid { get; set; } = true;
        public ControllerDocument Added { get; private set; }
        public ControllerDocument Edited { get; private set; }
        public ControllerDocument Duplicated { get; private set; }
        public ControllerDocument ById { get; set; }
        public string RemovedId { get; private set; }
        public string MovedUpId { get; private set; }
        public string MovedDownId { get; private set; }
        public Form ValidationForm { get; private set; }

        public ICollection<ControllerDocument> GetAll<T>(string orderBy = null, bool descending = false) => [];
        public void MoveUp(string id) => MovedUpId = id;
        public void MoveDown(string id) => MovedDownId = id;
        public CmsListReponse<ControllerDocument> GetPaged<T>(CmsListRequest request) => new() { Items = [] };
        public ControllerDocument GetById(string id) => ById;
        public void Add(ControllerDocument model, List<IFormFile> files, List<string> addedFiles) => Added = model;
        public void Duplicate(ControllerDocument model, List<IFormFile> files, List<string> addedFiles) => Duplicated = model;
        public void Edit(ControllerDocument model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles) => Edited = model;
        public void Remove(string id) => RemovedId = id;
        public AddForm BuildAddForm() => new(FormClassMapping.GetMapping<ControllerDocument>());
        public EditForm BuildEditForm(string id) => new(FormClassMapping.GetMapping<ControllerDocument>(), ById);
        public DuplicateForm BuildDuplicateForm(string id) => new(FormClassMapping.GetMapping<ControllerDocument>(), ById);

        public bool Validate(ControllerDocument model, bool isEdit, List<string> addedFiles, out Form form)
        {
            ValidationForm = isEdit
                ? new EditForm(FormClassMapping.GetMapping<ControllerDocument>(), model)
                : new AddForm(FormClassMapping.GetMapping<ControllerDocument>(), model);
            form = ValidationForm;
            return IsValid;
        }
    }

    private sealed class RecordingFlashMessage : IFlashMessage
    {
        public string LastConfirmation { get; private set; }
        public void Confirmation(string message) => LastConfirmation = message;
        public void Confirmation(string title, string message) => LastConfirmation = $"{title}: {message}";
        public void Danger(string message) { }
        public void Danger(string title, string message) { }
    }
}
