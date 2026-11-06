using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
    public async Task Add_post_calls_service_flashes_confirmation_and_redirects_to_mapping_index()
    {
        var controller = CreateControllerWithTestCancellation();
        var model = new ControllerDocument { Title = "Synthetic page" };

        var result = await controller.Add(model, [], []);

        Assert.Same(model, _service.Added);
        Assert.Equal("Added 'Synthetic page'", _flash.LastConfirmation);
        Assert.Equal("/Cms/SyntheticPages", Assert.IsType<RedirectResult>(result).Url);
    }

    [Fact]
    public async Task Add_post_returns_form_without_writing_when_validation_fails()
    {
        _service.IsValid = false;
        var controller = CreateControllerWithTestCancellation();

        var result = await controller.Add(new ControllerDocument { Title = "Invalid" }, [], []);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Cms/Add", view.ViewName);
        Assert.Same(_service.ValidationForm, view.Model);
        Assert.Null(_service.Added);
        Assert.Null(_flash.LastConfirmation);
    }

    [Fact]
    public async Task Edit_duplicate_remove_and_order_actions_delegate_and_preserve_redirects()
    {
        var existing = new ControllerDocument { Id = "existing", Title = "Existing page" };
        _service.ById = existing;
        var controller = CreateControllerWithTestCancellation();

        var edit = await controller.Edit(existing, [], [], []);
        var duplicate = await controller.Duplicate(existing, [], []);
        var remove = await controller.Remove(existing.Id);
        var moveUp = await controller.MoveUp(existing.Id);
        var moveDown = await controller.MoveDown(existing.Id);

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
    public async Task Disabled_capability_throws_not_supported(string disableMethod, string action)
    {
        var mapping = FormClassMapping.GetMapping<ControllerDocument>();
        typeof(FormClassMapping).GetMethod(disableMethod)?.Invoke(mapping, null);
        _service.ById = new ControllerDocument { Id = "existing", Title = "Existing" };
        var controller = CreateControllerWithTestCancellation();

        await Assert.ThrowsAsync<NotSupportedException>(() => action switch
        {
            nameof(CmsController<ControllerDocument>.Add) => controller.Add(),
            nameof(CmsController<ControllerDocument>.Edit) => controller.Edit("existing"),
            nameof(CmsController<ControllerDocument>.Remove) => controller.Remove("existing"),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        });
    }

    [Fact]
    public async Task Duplicate_and_ordering_require_explicit_mapping_capabilities()
    {
        FormClassMapping.Maps.Clear();
        FormClassMapping.RegisterClassMapping<ControllerDocument>(map =>
        {
            map.SetControllerName("SyntheticPages");
            map.SetDisplayName("Synthetic Page");
            map.EnableDuplicate();
        });
        var controller = CreateControllerWithTestCancellation();

        await Assert.ThrowsAsync<NotSupportedException>(() => controller.MoveUp("id"));
        await Assert.ThrowsAsync<NotSupportedException>(() => controller.MoveDown("id"));

        FormClassMapping.Maps.Clear();
        FormClassMapping.RegisterClassMapping<ControllerDocument>(map =>
        {
            map.SetControllerName("SyntheticPages");
            map.SetDisplayName("Synthetic Page");
        });
        controller = CreateControllerWithTestCancellation();
        await Assert.ThrowsAsync<NotSupportedException>(() => controller.Duplicate("id"));
    }

    [Fact]
    public async Task Index_passes_request_cancellation_to_async_service()
    {
        using var cancellation = new CancellationTokenSource();
        var controller = CreateController(cancellation.Token);

        await controller.Index(new CmsListRequest());

        Assert.Equal(cancellation.Token, _service.LastCancellationToken);
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

    private CmsController<ControllerDocument> CreateControllerWithTestCancellation()
    {
        return CreateController(TestContext.Current.CancellationToken);
    }

    private CmsController<ControllerDocument> CreateController(CancellationToken cancellationToken)
    {
        var context = new DefaultHttpContext
        {
            RequestAborted = cancellationToken
        };
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("localhost");
        context.Request.Path = "/Cms/SyntheticPages";

        return new CmsController<ControllerDocument>(_service, _flash)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = context
            }
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
        public CancellationToken LastCancellationToken { get; private set; }

        public ICollection<ControllerDocument> GetAll<T>(string orderBy = null, bool descending = false) => [];
        public Task<ICollection<ControllerDocument>> GetAllAsync<T>(string orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult<ICollection<ControllerDocument>>([]);
        }
        public void MoveUp(string id) => MovedUpId = id;
        public Task MoveUpAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            MovedUpId = id;
            return Task.CompletedTask;
        }
        public void MoveDown(string id) => MovedDownId = id;
        public Task MoveDownAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            MovedDownId = id;
            return Task.CompletedTask;
        }
        public CmsListReponse<ControllerDocument> GetPaged<T>(CmsListRequest request) => new() { Items = [] };
        public Task<CmsListReponse<ControllerDocument>> GetPagedAsync<T>(CmsListRequest request, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult(new CmsListReponse<ControllerDocument> { Items = [] });
        }
        public ControllerDocument GetById(string id) => ById;
        public Task<ControllerDocument> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult(ById);
        }
        public void Add(ControllerDocument model, List<IFormFile> files, List<string> addedFiles) => Added = model;
        public Task AddAsync(ControllerDocument model, List<IFormFile> files, List<string> addedFiles, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            Added = model;
            return Task.CompletedTask;
        }
        public void Duplicate(ControllerDocument model, List<IFormFile> files, List<string> addedFiles) => Duplicated = model;
        public Task DuplicateAsync(ControllerDocument model, List<IFormFile> files, List<string> addedFiles, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            Duplicated = model;
            return Task.CompletedTask;
        }
        public void Edit(ControllerDocument model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles) => Edited = model;
        public Task EditAsync(ControllerDocument model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            Edited = model;
            return Task.CompletedTask;
        }
        public void Remove(string id) => RemovedId = id;
        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            RemovedId = id;
            return Task.CompletedTask;
        }
        public AddForm BuildAddForm() => new(FormClassMapping.GetMapping<ControllerDocument>());
        public Task<AddForm> BuildAddFormAsync(CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult(BuildAddForm());
        }
        public EditForm BuildEditForm(string id) => new(FormClassMapping.GetMapping<ControllerDocument>(), ById);
        public Task<EditForm> BuildEditFormAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult(BuildEditForm(id));
        }
        public DuplicateForm BuildDuplicateForm(string id) => new(FormClassMapping.GetMapping<ControllerDocument>(), ById);
        public Task<DuplicateForm> BuildDuplicateFormAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult(BuildDuplicateForm(id));
        }

        public bool Validate(ControllerDocument model, bool isEdit, List<string> addedFiles, out Form form)
        {
            ValidationForm = isEdit
                ? new EditForm(FormClassMapping.GetMapping<ControllerDocument>(), model)
                : new AddForm(FormClassMapping.GetMapping<ControllerDocument>(), model);
            form = ValidationForm;
            return IsValid;
        }

        public Task<(bool IsValid, Form Form)> ValidateAsync(ControllerDocument model, bool isEdit, List<string> addedFiles, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            var isValid = Validate(model, isEdit, addedFiles, out var form);
            return Task.FromResult((isValid, form));
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
