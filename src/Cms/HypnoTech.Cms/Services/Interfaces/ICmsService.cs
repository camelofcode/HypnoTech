using System.Collections.Generic;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Dtos.Response;
using HypnoTech.Cms.Forms;
using Microsoft.AspNetCore.Http;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface ICmsService<TClass> where TClass : class
    {
        ICollection<TClass> GetAll<T>(string orderBy = null, bool descending = false);

        void MoveUp(string id);

        void MoveDown(string id);

        CmsListReponse<TClass> GetPaged<T>(CmsListRequest request);

        TClass GetById(string id);

        void Add(TClass model, List<IFormFile> files, List<string> addedFiles);

        void Duplicate(TClass model, List<IFormFile> files, List<string> addedFiles);

        void Edit(TClass model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles);

        void Remove(string id);

        AddForm BuildAddForm();

        EditForm BuildEditForm(string id);

        DuplicateForm BuildDuplicateForm(string id);

        bool Validate(TClass model, bool isEdit, List<string> addedFiles, out Form form);
    }
}
