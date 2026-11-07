using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class ImageUploadInputMapping : FormMemberMapping, IFileUpload
    {
        public List<string> AllowedFileTypes { get; private set; }
        public string TargetDirectory { get; private set; }

        public int MaxHeight { get; private set; }
        public int MaxWidth { get; private set; }

        public ImageUploadInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.ImageUpload;
            AllowedFileTypes = new List<string> { "image/png", "image/jpeg" };
            TargetDirectory = "Misc";
        }

        public override FormControl GetControl()
        {
            return new ImageUploadFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new ImageUploadFormControl(this, obj);
        }

        public IFileUpload SetAllowedFileTypes(params string[] allowedFileTypes)
        {
            AllowedFileTypes = allowedFileTypes.ToList();
            return this;
        }

        public IFileUpload SetTargetDirectory(string targetDirectory)
        {
            TargetDirectory = targetDirectory;
            return this;
        }

        public ImageUploadInputMapping SetImageSize(int maxWidth, int maxHeight)
        {
            MaxHeight = maxHeight;
            MaxWidth = maxWidth;
            return this;
        }
    }
}
