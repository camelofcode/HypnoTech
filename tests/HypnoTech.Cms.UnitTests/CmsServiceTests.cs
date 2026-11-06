using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Dtos.Response;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.Services;
using HypnoTech.Cms.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HypnoTech.Cms.UnitTests;

public sealed class CmsServiceTests : IDisposable
{
    private readonly RecordingRepository _repository = new();
    private readonly RecordingImageUploadService _imageService = new();
    private readonly string _uploadPath = Path.Combine(Path.GetTempPath(), $"hypnotech-tests-{Guid.NewGuid():N}");
    private readonly CmsService<ServiceDocument> _service;

    public CmsServiceTests()
    {
        FormClassMapping.Maps.Clear();
        FormClassMapping.RegisterClassMapping<ServiceDocument>(mapping =>
        {
            mapping.SetControllerName("ServiceDocuments");
            mapping.SetDisplayName("Service Document");
            mapping.MapHiddenInputMember(document => document.Id);
            mapping.MapTextInputMember(document => document.Title).Required();
            mapping.MapDateInputMember(document => document.Date);
            mapping.MapGeneratedMember(document => document.Slug, document => document.Title.ToLowerInvariant().Replace(' ', '-'));
            mapping.MapCheckboxInputMember(document => document.Enabled)
                .HideWhenUnchecked(document => document.RelatedValue);
            mapping.MapFileUploadInputMember(document => document.Attachment)
                .SetTargetDirectory("Documents");
            mapping.MapImageUploadInputMember(document => document.Image)
                .SetTargetDirectory("Covers");
        });
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["UploadPath"] = _uploadPath
            })
            .Build();
        _service = new CmsService<ServiceDocument>(
            _repository,
            _imageService,
            configuration,
            new EmptySelectListRepository());
    }

    public void Dispose()
    {
        FormClassMapping.Maps.Clear();
        if (Directory.Exists(_uploadPath))
        {
            Directory.Delete(_uploadPath, recursive: true);
        }
    }

    [Fact]
    public void Add_applies_generated_date_and_checkbox_rules_before_repository_write()
    {
        var model = new ServiceDocument
        {
            Title = "Synthetic Document",
            Date = new DateTime(2026, 2, 3),
            Enabled = false,
            RelatedValue = "must be cleared"
        };

        _service.Add(model, [], []);

        Assert.Same(model, _repository.Added);
        Assert.Equal("synthetic-document", model.Slug);
        Assert.Equal(12, model.Date?.Hour);
        Assert.Null(model.RelatedValue);
    }

    [Fact]
    public void Edit_applies_transformations_and_delegates_to_repository()
    {
        var model = new ServiceDocument
        {
            Id = "synthetic-id",
            Title = "Edited Document",
            Date = new DateTime(2026, 3, 4),
            Enabled = true,
            RelatedValue = "preserved"
        };

        _service.Edit(model, [], [], []);

        Assert.Same(model, _repository.Edited);
        Assert.Equal("edited-document", model.Slug);
        Assert.Equal(12, model.Date?.Hour);
        Assert.Equal("preserved", model.RelatedValue);
    }

    [Fact]
    public void Form_building_and_validation_preserve_routes_controls_and_required_rules()
    {
        var addForm = _service.BuildAddForm();
        var valid = _service.Validate(new ServiceDocument(), isEdit: false, [], out var validatedForm);

        Assert.Equal("/Cms/ServiceDocuments", addForm.BackUrl);
        Assert.Equal("/Cms/ServiceDocuments/Add", addForm.PostUrl);
        Assert.DoesNotContain(addForm.Controls, control => control.Name == nameof(ServiceDocument.Id));
        Assert.False(valid);
        Assert.True(validatedForm.HasBeenValidated);
        Assert.Contains(validatedForm.Controls, control =>
            control.Name == nameof(ServiceDocument.Title) && !control.IsValid);
    }

    [Fact]
    public void Remove_without_file_mappings_delegates_without_loading_entity()
    {
        FormClassMapping.GetMapping<ServiceDocument>().MemberMappings =
            new List<FormMemberMapping>(FormClassMapping.GetMapping<ServiceDocument>().MemberMappings
                .Where(mapping => !mapping.IsFileInput));

        _service.Remove("synthetic-id");

        Assert.Equal("synthetic-id", _repository.RemovedId);
        Assert.Equal(0, _repository.GetByIdCalls);
    }

    [Fact]
    public void Add_writes_file_uploads_to_isolated_storage_and_delegates_images()
    {
        var audioBytes = Encoding.UTF8.GetBytes("synthetic audio payload");
        var audio = new FormFile(new MemoryStream(audioBytes), 0, audioBytes.Length, "file", "sample.mp3")
        {
            Headers = new HeaderDictionary(),
            ContentType = "audio/mpeg"
        };
        var image = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "image", "cover.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
        var model = new ServiceDocument { Title = "Uploads" };

        _service.Add(model, [audio, image], [nameof(ServiceDocument.Attachment), nameof(ServiceDocument.Image)]);

        Assert.Equal("/Files/Documents/sample.mp3", model.Attachment);
        Assert.Equal(audioBytes, File.ReadAllBytes(Path.Combine(_uploadPath, "Files", "Documents", "sample.mp3")));
        Assert.Equal("/Images/Covers/synthetic.png", model.Image);
        Assert.Equal("Covers", _imageService.LastSubdirectory);
    }

    [Fact]
    public async Task AddAsync_applies_transformations_uploads_files_and_propagates_cancellation()
    {
        var audioBytes = Encoding.UTF8.GetBytes("synthetic async audio payload");
        var audio = new FormFile(new MemoryStream(audioBytes), 0, audioBytes.Length, "file", "async.mp3")
        {
            Headers = new HeaderDictionary(),
            ContentType = "audio/mpeg"
        };
        var model = new ServiceDocument
        {
            Title = "Async Document",
            Date = new DateTime(2026, 4, 5),
            Enabled = false,
            RelatedValue = "must be cleared"
        };
        using var cancellation = new CancellationTokenSource();

        await _service.AddAsync(
            model,
            [audio],
            [nameof(ServiceDocument.Attachment)],
            cancellation.Token);

        Assert.Same(model, _repository.Added);
        Assert.Equal(cancellation.Token, _repository.LastCancellationToken);
        Assert.Equal("async-document", model.Slug);
        Assert.Equal(12, model.Date?.Hour);
        Assert.Null(model.RelatedValue);
        Assert.Equal(audioBytes, await File.ReadAllBytesAsync(
            Path.Combine(_uploadPath, "Files", "Documents", "async.mp3"),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Async_form_building_and_validation_preserve_form_contract()
    {
        var addForm = await _service.BuildAddFormAsync(TestContext.Current.CancellationToken);
        var validation = await _service.ValidateAsync(
            new ServiceDocument(),
            isEdit: false,
            [],
            TestContext.Current.CancellationToken);

        Assert.Equal("/Cms/ServiceDocuments", addForm.BackUrl);
        Assert.Equal("/Cms/ServiceDocuments/Add", addForm.PostUrl);
        Assert.False(validation.IsValid);
        Assert.True(validation.Form.HasBeenValidated);
        Assert.Contains(validation.Form.Controls, control =>
            control.Name == nameof(ServiceDocument.Title) && !control.IsValid);
    }

    public sealed class ServiceDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime? Date { get; set; }
        public string Slug { get; set; }
        public bool Enabled { get; set; }
        public string RelatedValue { get; set; }
        public string Attachment { get; set; }
        public string Image { get; set; }
    }

    private sealed class RecordingRepository : ICmsRepository<ServiceDocument>
    {
        public ServiceDocument Added { get; private set; }
        public ServiceDocument Edited { get; private set; }
        public string RemovedId { get; private set; }
        public int GetByIdCalls { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public ICollection<ServiceDocument> GetAll<T>(string orderBy = null, bool descending = false) => [];
        public Task<ICollection<ServiceDocument>> GetAllAsync<T>(string orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult<ICollection<ServiceDocument>>([]);
        }
        public CmsListReponse<ServiceDocument> GetPaged<T>(CmsListRequest request) => new() { Items = [] };
        public Task<CmsListReponse<ServiceDocument>> GetPagedAsync<T>(CmsListRequest request, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult(new CmsListReponse<ServiceDocument> { Items = [] });
        }
        public void MoveUp(string id) { }
        public Task MoveUpAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
        public void MoveDown(string id) { }
        public Task MoveDownAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
        public ServiceDocument GetById(string id)
        {
            GetByIdCalls++;
            return Added;
        }
        public Task<ServiceDocument> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            GetByIdCalls++;
            return Task.FromResult(Added);
        }
        public void Add(ServiceDocument model) => Added = model;
        public Task AddAsync(ServiceDocument model, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            Added = model;
            return Task.CompletedTask;
        }
        public void Edit(ServiceDocument model) => Edited = model;
        public Task EditAsync(ServiceDocument model, CancellationToken cancellationToken = default)
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
    }

    private sealed class RecordingImageUploadService : ICmsImageUploadService
    {
        public string LastSubdirectory { get; private set; }

        public string AddImage(IFormFile file, string subdirectory, int maxHeight, int maxWidth)
        {
            LastSubdirectory = subdirectory;
            return $"/Images/{subdirectory}/synthetic.png";
        }

        public Task<string> AddImageAsync(IFormFile file, string subdirectory, int maxHeight, int maxWidth, CancellationToken cancellationToken = default)
        {
            LastSubdirectory = subdirectory;
            return Task.FromResult($"/Images/{subdirectory}/synthetic.png");
        }

        public string RemoveImage(string name, string subdirectory) => null;
    }

    private sealed class EmptySelectListRepository : ISelectListRepository
    {
        public List<object> GetSelectListItems(Type type) => [];
        public Task<List<object>> GetSelectListItemsAsync(Type type, CancellationToken cancellationToken = default) =>
            Task.FromResult(new List<object>());
    }
}
