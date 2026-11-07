using System.Collections.Generic;
using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms.Controls
{
    public class ImageUploadFormControl : FormControl, IHasValue
    {
        public string ImagePath { get; }

        public object Value => ImagePath;

        public bool HasValue => !string.IsNullOrEmpty(ImagePath);

        public string ViewModelName => $"modelFor{Name}";

        public string Anchor => $"{Id}-anchor";

        public List<string> AllowedFileTypes;

        public ImageUploadFormControl(ImageUploadInputMapping mapping) : base(mapping)
        {
            AllowedFileTypes = mapping.AllowedFileTypes;
        }

        public ImageUploadFormControl(ImageUploadInputMapping mapping, object obj) : base(mapping)
        {
            AllowedFileTypes = mapping.AllowedFileTypes;
            ImagePath = mapping.GetPropertyValue(obj)?.ToString();
        }

        public override string EditorTemplate => "ImageUpload";
    }
}
