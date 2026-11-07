using System.Collections.Generic;
using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms.Controls
{
    public class FileUploadFormControl : FormControl, IHasValue
    {
        public string FilePath { get; }

        public bool HasValue => !string.IsNullOrEmpty(FilePath);

        public object Value => FilePath;

        public string ViewModelName => $"modelFor{Name}";

        public string Anchor => $"{Id}-anchor";

        public List<string> AllowedFileTypes;

        public FileUploadFormControl(FileUploadInputMapping mapping) : base(mapping)
        {
            AllowedFileTypes = mapping.AllowedFileTypes;
        }

        public FileUploadFormControl(FileUploadInputMapping mapping, object obj) : base(mapping)
        {
            AllowedFileTypes = mapping.AllowedFileTypes;
            FilePath = mapping.GetPropertyValue(obj)?.ToString();
        }

        public override string EditorTemplate => "FileUpload";

    }
}
