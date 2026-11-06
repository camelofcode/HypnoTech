using System.Collections.Generic;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Dtos.Response;
using HypnoTech.Cms.Forms;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface ICmsService<TClass> where TClass : class
    {
        ICollection<TClass> GetAll<T>(string orderBy = null, bool descending = false);

        Task<ICollection<TClass>> GetAllAsync<T>(string orderBy = null, bool descending = false, CancellationToken cancellationToken = default);

        void MoveUp(string id);

        Task MoveUpAsync(string id, CancellationToken cancellationToken = default);

        void MoveDown(string id);

        Task MoveDownAsync(string id, CancellationToken cancellationToken = default);

        CmsListReponse<TClass> GetPaged<T>(CmsListRequest request);

        Task<CmsListReponse<TClass>> GetPagedAsync<T>(CmsListRequest request, CancellationToken cancellationToken = default);

        TClass GetById(string id);

        Task<TClass> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        void Add(TClass model, List<IFormFile> files, List<string> addedFiles);

        Task AddAsync(TClass model, List<IFormFile> files, List<string> addedFiles, CancellationToken cancellationToken = default);

        void Duplicate(TClass model, List<IFormFile> files, List<string> addedFiles);

        Task DuplicateAsync(TClass model, List<IFormFile> files, List<string> addedFiles, CancellationToken cancellationToken = default);

        void Edit(TClass model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles);

        Task EditAsync(TClass model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles, CancellationToken cancellationToken = default);

        void Remove(string id);

        Task RemoveAsync(string id, CancellationToken cancellationToken = default);

        AddForm BuildAddForm();

        Task<AddForm> BuildAddFormAsync(CancellationToken cancellationToken = default);

        EditForm BuildEditForm(string id);

        Task<EditForm> BuildEditFormAsync(string id, CancellationToken cancellationToken = default);

        DuplicateForm BuildDuplicateForm(string id);

        Task<DuplicateForm> BuildDuplicateFormAsync(string id, CancellationToken cancellationToken = default);

        bool Validate(TClass model, bool isEdit, List<string> addedFiles, out Form form);

        Task<(bool IsValid, Form Form)> ValidateAsync(TClass model, bool isEdit, List<string> addedFiles, CancellationToken cancellationToken = default);
    }
}
