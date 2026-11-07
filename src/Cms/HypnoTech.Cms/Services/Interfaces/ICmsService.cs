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
        Task<ICollection<TClass>> GetAllAsync<T>(string orderBy = null, bool descending = false, CancellationToken cancellationToken = default);

        Task MoveUpAsync(string id, CancellationToken cancellationToken = default);

        Task MoveDownAsync(string id, CancellationToken cancellationToken = default);

        Task<CmsListReponse<TClass>> GetPagedAsync<T>(CmsListRequest request, CancellationToken cancellationToken = default);

        Task<TClass> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        Task AddAsync(TClass model, List<IFormFile> files, List<string> addedFiles, CancellationToken cancellationToken = default);

        Task DuplicateAsync(TClass model, List<IFormFile> files, List<string> addedFiles, CancellationToken cancellationToken = default);

        Task EditAsync(TClass model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles, CancellationToken cancellationToken = default);

        Task RemoveAsync(string id, CancellationToken cancellationToken = default);

        Task<AddForm> BuildAddFormAsync(CancellationToken cancellationToken = default);

        Task<EditForm> BuildEditFormAsync(string id, CancellationToken cancellationToken = default);

        Task<DuplicateForm> BuildDuplicateFormAsync(string id, CancellationToken cancellationToken = default);

        Task<(bool IsValid, Form Form)> ValidateAsync(TClass model, bool isEdit, List<string> addedFiles, CancellationToken cancellationToken = default);
    }
}
