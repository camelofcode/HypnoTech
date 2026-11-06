using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Dtos.Response;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Cms.Forms;
using HypnoTech.Cms.Forms.Controls;
using HypnoTech.Cms.Helpers;
using HypnoTech.Cms.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StringExtensions = HypnoTech.Common.Extensions.StringExtensions;

namespace HypnoTech.Cms.Services
{
    public class CmsService<TClass> : ICmsService<TClass> where TClass : class
    {
        private readonly ICmsRepository<TClass> _repository;
        private readonly ICmsImageUploadService _imageUploadService;
        private readonly string _baseFilePath;
        private readonly ISelectListRepository _selectListRepository;

        public CmsService(ICmsRepository<TClass> repository, ICmsImageUploadService imageUploadService, IConfiguration config, ISelectListRepository selectListRepository)
        {
            _repository = repository;
            _imageUploadService = imageUploadService;
            _selectListRepository = selectListRepository;
            _baseFilePath = config.UploadPath();
        }

        public ICollection<TClass> GetAll<T>(string orderBy = null, bool descending = false)
        {
            return _repository.GetAll<T>(orderBy, descending);
        }

        public Task<ICollection<TClass>> GetAllAsync<T>(
            string orderBy = null,
            bool descending = false,
            CancellationToken cancellationToken = default)
        {
            return _repository.GetAllAsync<T>(orderBy, descending, cancellationToken);
        }

        public void MoveUp(string id)
        {
            _repository.MoveUp(id);
        }

        public Task MoveUpAsync(string id, CancellationToken cancellationToken = default)
        {
            return _repository.MoveUpAsync(id, cancellationToken);
        }

        public void MoveDown(string id)
        {
            _repository.MoveDown(id);
        }

        public Task MoveDownAsync(string id, CancellationToken cancellationToken = default)
        {
            return _repository.MoveDownAsync(id, cancellationToken);
        }

        public TClass GetById(string id)
        {
            return _repository.GetById(id);
        }

        public Task<TClass> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return _repository.GetByIdAsync(id, cancellationToken);
        }

        public CmsListReponse<TClass> GetPaged<T>(CmsListRequest request)
        {
            return _repository.GetPaged<T>(request);
        }

        public Task<CmsListReponse<TClass>> GetPagedAsync<T>(
            CmsListRequest request,
            CancellationToken cancellationToken = default)
        {
            return _repository.GetPagedAsync<T>(request, cancellationToken);
        }

        public void Add(TClass model, List<IFormFile> files, List<string> addedFiles)
        {
            HandleFileUploads(model, files, addedFiles);
            PopulateGeneratedMembers(model);
            NormaliseDates(model);
            ApplyDefaultValuesToHiddenFields(model);

            _repository.Add(model);
        }

        public async Task AddAsync(
            TClass model,
            List<IFormFile> files,
            List<string> addedFiles,
            CancellationToken cancellationToken = default)
        {
            await HandleFileUploadsAsync(model, files, addedFiles, cancellationToken);
            PopulateGeneratedMembers(model);
            NormaliseDates(model);
            ApplyDefaultValuesToHiddenFields(model);

            await _repository.AddAsync(model, cancellationToken);
        }

        public void Duplicate(TClass model, List<IFormFile> files, List<string> addedFiles)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (mapping.CloneFilesWhenDuplicating)
            {
                CloneFiles(model);
            }
            Add(model, files, addedFiles);
        }

        public async Task DuplicateAsync(
            TClass model,
            List<IFormFile> files,
            List<string> addedFiles,
            CancellationToken cancellationToken = default)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (mapping.CloneFilesWhenDuplicating)
            {
                await CloneFilesAsync(model, cancellationToken);
            }
            await AddAsync(model, files, addedFiles, cancellationToken);
        }

        public void Edit(TClass model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles)
        {
            HandleRemovedFiles(removedFiles);
            HandleFileUploads(model, files, addedFiles);
            PopulateGeneratedMembers(model);
            NormaliseDates(model);
            ApplyDefaultValuesToHiddenFields(model);

            _repository.Edit(model);
        }

        public async Task EditAsync(
            TClass model,
            List<IFormFile> files,
            List<string> addedFiles,
            List<string> removedFiles,
            CancellationToken cancellationToken = default)
        {
            HandleRemovedFiles(removedFiles);
            await HandleFileUploadsAsync(model, files, addedFiles, cancellationToken);
            PopulateGeneratedMembers(model);
            NormaliseDates(model);
            ApplyDefaultValuesToHiddenFields(model);

            await _repository.EditAsync(model, cancellationToken);
        }

        public void Remove(string id)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (mapping.MemberMappings.Any(p => p.IsFileInput))
            {
                var model = GetById(id);
                foreach (var fileInputMember in mapping.MemberMappings.Where(p => p.IsFileInput))
                {
                    var file = typeof(TClass).GetProperty(fileInputMember.MemberName)?.GetValue(model)?.ToString();
                    if (!string.IsNullOrEmpty(file))
                    {
                        HandleRemovedFile(file);
                    }
                }
            }
            _repository.Remove(id);
        }

        public async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (mapping.MemberMappings.Any(p => p.IsFileInput))
            {
                var model = await GetByIdAsync(id, cancellationToken);
                foreach (var fileInputMember in mapping.MemberMappings.Where(p => p.IsFileInput))
                {
                    var file = typeof(TClass).GetProperty(fileInputMember.MemberName)?.GetValue(model)?.ToString();
                    if (!string.IsNullOrEmpty(file))
                    {
                        HandleRemovedFile(file);
                    }
                }
            }
            await _repository.RemoveAsync(id, cancellationToken);
        }

        public AddForm BuildAddForm()
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            var form = new AddForm(mapping);
            BuildSelectLists(form);
            return form;
        }

        public async Task<AddForm> BuildAddFormAsync(CancellationToken cancellationToken = default)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            var form = new AddForm(mapping);
            await BuildSelectListsAsync(form, cancellationToken);
            return form;
        }

        public EditForm BuildEditForm(string id)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            var entity = GetById(id);
            var form = new EditForm(mapping, entity);
            BuildSelectLists(form);
            return form;
        }

        public async Task<EditForm> BuildEditFormAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            var entity = await GetByIdAsync(id, cancellationToken);
            var form = new EditForm(mapping, entity);
            await BuildSelectListsAsync(form, cancellationToken);
            return form;
        }

        public DuplicateForm BuildDuplicateForm(string id)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            var entity = GetById(id);
            var form = new DuplicateForm(mapping, entity);
            BuildSelectLists(form);
            return form;
        }

        public async Task<DuplicateForm> BuildDuplicateFormAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            var entity = await GetByIdAsync(id, cancellationToken);
            var form = new DuplicateForm(mapping, entity);
            await BuildSelectListsAsync(form, cancellationToken);
            return form;
        }

        public bool Validate(TClass model, bool isEdit, List<string> addedFiles, out Form form)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            form = isEdit ? (Form)new EditForm(mapping, model) : new AddForm(mapping, model);
            BuildSelectLists(form);
            return form.Validate(addedFiles);
        }

        public async Task<(bool IsValid, Form Form)> ValidateAsync(
            TClass model,
            bool isEdit,
            List<string> addedFiles,
            CancellationToken cancellationToken = default)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            Form form = isEdit ? new EditForm(mapping, model) : new AddForm(mapping, model);
            await BuildSelectListsAsync(form, cancellationToken);
            return (form.Validate(addedFiles), form);
        }

        private void BuildSelectLists(Form form)
        {
            foreach (var control in form.Controls.Where(p => p is SelectListInputFormControl))
            {
                var selectListControl = (SelectListInputFormControl)control;
                selectListControl.MapValues(_selectListRepository.GetSelectListItems(selectListControl.ListType));
            }
            foreach (var control in form.Controls.Where(p => p is NestedSelectListInputFormControl))
            {
                var selectListControl = (NestedSelectListInputFormControl)control;
                selectListControl.MapValues(_selectListRepository.GetSelectListItems(selectListControl.ListType));
            }
        }

        private async Task BuildSelectListsAsync(Form form, CancellationToken cancellationToken)
        {
            foreach (var control in form.Controls.Where(p => p is SelectListInputFormControl))
            {
                var selectListControl = (SelectListInputFormControl)control;
                selectListControl.MapValues(await _selectListRepository.GetSelectListItemsAsync(
                    selectListControl.ListType,
                    cancellationToken));
            }
            foreach (var control in form.Controls.Where(p => p is NestedSelectListInputFormControl))
            {
                var selectListControl = (NestedSelectListInputFormControl)control;
                selectListControl.MapValues(await _selectListRepository.GetSelectListItemsAsync(
                    selectListControl.ListType,
                    cancellationToken));
            }
        }

        private void CloneFiles(TClass model)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();

            foreach (var file in mapping.MemberMappings.Where(p => p is ImageUploadInputMapping || p is FileUploadInputMapping))
            {
                var currentValue = typeof(TClass).GetProperty(file.MemberName)?.GetValue(model)?.ToString();
                if (!string.IsNullOrEmpty(currentValue))
                {
                    var newValue = CloneFile(currentValue);
                    typeof(TClass).GetProperty(file.MemberName)?.SetValue(model, newValue);
                }
            }

            foreach (var gallery in mapping.MemberMappings.Where(p => p is PhotoGalleryInputMapping))
            {
                if (!(gallery is PhotoGalleryInputMapping strictGallery)) 
                {
                    return;
                }

                var photos = typeof(TClass).GetProperty(strictGallery.MemberName)?.GetValue(model) as IList;

                for (int i = 0; i < photos?.Count; i++)
                {
                    var item = photos[i];
                    var currentValue = item?.GetType().GetProperty(strictGallery.ImagePathName)?.GetValue(item)?.ToString();
                    if (!string.IsNullOrEmpty(currentValue))
                    {
                        var newValue = CloneFile(currentValue);
                        strictGallery.SetImage(model, i, newValue);
                    }
                }
            }
        }

        private async Task CloneFilesAsync(TClass model, CancellationToken cancellationToken)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();

            foreach (var file in mapping.MemberMappings.Where(p => p is ImageUploadInputMapping || p is FileUploadInputMapping))
            {
                var currentValue = typeof(TClass).GetProperty(file.MemberName)?.GetValue(model)?.ToString();
                if (!string.IsNullOrEmpty(currentValue))
                {
                    var newValue = await CloneFileAsync(currentValue, cancellationToken);
                    typeof(TClass).GetProperty(file.MemberName)?.SetValue(model, newValue);
                }
            }

            foreach (var gallery in mapping.MemberMappings.Where(p => p is PhotoGalleryInputMapping))
            {
                if (!(gallery is PhotoGalleryInputMapping strictGallery))
                {
                    return;
                }

                var photos = typeof(TClass).GetProperty(strictGallery.MemberName)?.GetValue(model) as IList;

                for (int i = 0; i < photos?.Count; i++)
                {
                    var item = photos[i];
                    var currentValue = item?.GetType().GetProperty(strictGallery.ImagePathName)?.GetValue(item)?.ToString();
                    if (!string.IsNullOrEmpty(currentValue))
                    {
                        var newValue = await CloneFileAsync(currentValue, cancellationToken);
                        strictGallery.SetImage(model, i, newValue);
                    }
                }
            }
        }

        private string CloneFile(string path)
        {
            var fullPath = Path.Combine(_baseFilePath, StringExtensions.UrlToFilePath(path).Trim(Path.DirectorySeparatorChar));
            var clonePath = FileHelper.GetDeduplicatedPath(fullPath);

            File.Copy(fullPath, clonePath);

            return clonePath.Replace(_baseFilePath, string.Empty);
        }

        private async Task<string> CloneFileAsync(string path, CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_baseFilePath, StringExtensions.UrlToFilePath(path).Trim(Path.DirectorySeparatorChar));
            var clonePath = FileHelper.GetDeduplicatedPath(fullPath);

            await using var source = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                useAsync: true);
            await using var destination = new FileStream(
                clonePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);
            await source.CopyToAsync(destination, cancellationToken);

            return clonePath.Replace(_baseFilePath, string.Empty);
        }

        private void HandleFileUploads(TClass model, List<IFormFile> files, List<string> addedFiles)
        {
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var fileProperty = addedFiles[i];
                var fileIndex = 0;

                if (fileProperty.Contains("_"))
                {
                    var split = fileProperty.Split('_');
                    fileProperty = split[0];
                    fileIndex = int.Parse(split[1]);
                }

                var mapping = FormClassMapping.GetMapping<TClass>();
                var memberMapping = mapping.GetMember(fileProperty);

                ValidateFileType(file, memberMapping);

                if (memberMapping is ImageUploadInputMapping imageMapping)
                {
                    var path = _imageUploadService.AddImage(file, imageMapping.TargetDirectory, imageMapping.MaxHeight, imageMapping.MaxWidth);
                    typeof(TClass).GetProperty(fileProperty)?.SetValue(model, path);
                }
                else if (memberMapping is PhotoGalleryInputMapping photoGalleryMapping)
                {
                    var galleryName = StringExtensions.GenerateSlug(mapping.GetTitle(model));
                    
                    var path = _imageUploadService.AddImage(file, Path.Combine(photoGalleryMapping.TargetDirectory, galleryName), photoGalleryMapping.MaxHeight, photoGalleryMapping.MaxWidth);
                    photoGalleryMapping.SetImage(model.GetType().GetProperty(fileProperty)?.GetValue(model), fileIndex, path);
                }
                else if (memberMapping is FileUploadInputMapping fileMapping)
                {
                    var directory = Path.Combine(_baseFilePath, "Files", fileMapping.TargetDirectory);
                    Directory.CreateDirectory(directory);

                    var filePath = Path.Combine(directory, file.FileName);
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            var fileBytes = ms.ToArray();

                            File.WriteAllBytes(filePath, fileBytes);
                            var saveLocation = $"/Files/{fileMapping.TargetDirectory}/{file.FileName}";
                            typeof(TClass).GetProperty(fileProperty)?.SetValue(model, saveLocation);
                        }
                    }
                }
            }
        }

        private async Task HandleFileUploadsAsync(
            TClass model,
            List<IFormFile> files,
            List<string> addedFiles,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var fileProperty = addedFiles[i];
                var fileIndex = 0;

                if (fileProperty.Contains("_"))
                {
                    var split = fileProperty.Split('_');
                    fileProperty = split[0];
                    fileIndex = int.Parse(split[1]);
                }

                var mapping = FormClassMapping.GetMapping<TClass>();
                var memberMapping = mapping.GetMember(fileProperty);

                ValidateFileType(file, memberMapping);

                if (memberMapping is ImageUploadInputMapping imageMapping)
                {
                    var path = await _imageUploadService.AddImageAsync(
                        file,
                        imageMapping.TargetDirectory,
                        imageMapping.MaxHeight,
                        imageMapping.MaxWidth,
                        cancellationToken);
                    typeof(TClass).GetProperty(fileProperty)?.SetValue(model, path);
                }
                else if (memberMapping is PhotoGalleryInputMapping photoGalleryMapping)
                {
                    var galleryName = StringExtensions.GenerateSlug(mapping.GetTitle(model));

                    var path = await _imageUploadService.AddImageAsync(
                        file,
                        Path.Combine(photoGalleryMapping.TargetDirectory, galleryName),
                        photoGalleryMapping.MaxHeight,
                        photoGalleryMapping.MaxWidth,
                        cancellationToken);
                    photoGalleryMapping.SetImage(
                        model.GetType().GetProperty(fileProperty)?.GetValue(model),
                        fileIndex,
                        path);
                }
                else if (memberMapping is FileUploadInputMapping fileMapping)
                {
                    var directory = Path.Combine(_baseFilePath, "Files", fileMapping.TargetDirectory);
                    Directory.CreateDirectory(directory);

                    var filePath = Path.Combine(directory, file.FileName);
                    if (file.Length > 0)
                    {
                        await using var destination = new FileStream(
                            filePath,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.None,
                            81920,
                            useAsync: true);
                        await file.CopyToAsync(destination, cancellationToken);
                        var saveLocation = $"/Files/{fileMapping.TargetDirectory}/{file.FileName}";
                        typeof(TClass).GetProperty(fileProperty)?.SetValue(model, saveLocation);
                    }
                }
            }
        }

        private void HandleRemovedFiles(List<string> removedFiles)
        {
            foreach (var removedFile in removedFiles)
            {
                HandleRemovedFile(removedFile);
            }
        }

        private void HandleRemovedFile(string removedFile)
        {
            var parts = removedFile.Split("/");
            if (parts.Length == 4)
            {
                var deletePath = Path.Combine(_baseFilePath, parts[1], parts[2], parts[3]);
                if (File.Exists(deletePath))
                {
                    try
                    {
                        File.Delete(deletePath);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        private void PopulateGeneratedMembers(TClass model)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            foreach (var generated in mapping.MemberMappings.Where(p => p is GeneratedMemberMapping<TClass>))
            {
                ((GeneratedMemberMapping<TClass>)generated).Generate(model);
            }
        }

        private void ApplyDefaultValuesToHiddenFields(TClass model)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            foreach (var checkbox in mapping.MemberMappings.Where(p => p is CheckboxInputMapping))
            {
                var isChecked = (bool) checkbox.GetPropertyValue(model);
                var cast = (CheckboxInputMapping)checkbox;

                var propertiesToDefault = isChecked ? cast.HiddenWhenChecked : cast.HiddenWhenUnchecked;

                foreach (var propertyName in propertiesToDefault)
                {
                    var property = model.GetType().GetProperty(propertyName);
                    if (property != null)
                    {
                        property.SetValue(model, null);
                    }
                }
            }
        }

        private void NormaliseDates(TClass model)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            foreach (var date in mapping.MemberMappings.Where(p => p is DateInputMapping))
            {
                var property = model.GetType().GetProperty(date.MemberName);
                if (property != null)
                {
                    var dateTime = (DateTime?)property.GetValue(model);
                    if (dateTime.HasValue)
                    {
                        var value = dateTime.Value;
                        if (value.Hour == 0)
                        {
                            value = new DateTime(value.Year, value.Month, value.Day, 12, 0, 0);
                            property.SetValue(model, value);
                        }
                    }
                }    
            }
        }

        private static void ValidateFileType(IFormFile file, FormMemberMapping memberMapping)
        {
            if (memberMapping is IFileUpload upload)
            {
                if (!upload.AllowedFileTypes.Contains(file.ContentType))
                {
                    throw new Exception($"File type {file.ContentType} not allowed for {typeof(TClass).Name}.{memberMapping.MemberName}");
                }
            }
        }
    }
}
