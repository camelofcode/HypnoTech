using System;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.FormMemberImplementations;

namespace HypnoTech.Cms.Forms.Controls
{
    public class PhotoGalleryFormControl<TClass> : PhotoGalleryFormControl
    {
        public List<string> AllowedFileTypes { get; private set; }

        public string ListName { get; }

        public TClass ObjectTemplate => Activator.CreateInstance<TClass>();

        public List<TClass> Value { get; }

        public string ViewModelName => $"modelFor{Name}";

        public string Anchor => $"{Id}-anchor";

        public string ImagePathName { get; }

        public string ImagePathNameJs => char.ToLowerInvariant(ImagePathName[0]) + ImagePathName.Substring(1);

        public string CaptionName { get; }

        public string CaptionNameJs => char.ToLowerInvariant(CaptionName[0]) + CaptionName.Substring(1);

        public PhotoGalleryFormControl(PhotoGalleryInputMapping<TClass> mapping) : base(mapping)
        {
            ListName = mapping.MemberName;
            ImagePathName = mapping.ImagePathName;
            CaptionName = mapping.CaptionName;
            AllowedFileTypes = mapping.AllowedFileTypes;
        }

        public PhotoGalleryFormControl(PhotoGalleryInputMapping<TClass> mapping, object obj) : base(mapping)
        {
            ListName = mapping.MemberName;
            ImagePathName = mapping.ImagePathName;
            CaptionName = mapping.CaptionName;
            AllowedFileTypes = mapping.AllowedFileTypes;
            Value = (List<TClass>)mapping.GetPropertyValue(obj);
        }

        public override string EditorTemplate => "PhotoGalleryInput";

        public override bool HasImages(List<string> addedFiles)
        {
            return addedFiles.Any(p => p.StartsWith(ListName)) || (Value != null && Value.Any());
        }
    }

    public abstract class PhotoGalleryFormControl : FormControl
    {
        protected PhotoGalleryFormControl(FormMemberMapping mapping) : base(mapping)
        {
        }

        public abstract bool HasImages(List<string> addedFiles);
    }
}
