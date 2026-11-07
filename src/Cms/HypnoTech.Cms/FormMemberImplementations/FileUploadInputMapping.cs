using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public class FileUploadInputMapping : FormMemberMapping, IFileUpload
    {
        public List<string> AllowedFileTypes { get; private set; }
        public string TargetDirectory { get; private set; }

        public FileUploadInputMapping(MemberInfo memberInfo, string accessPath, FormClassMapping classMapping) : base(memberInfo, accessPath, classMapping)
        {
            InputType = InputType.FileUpload;
            AllowedFileTypes = new List<string> { "audio/mp3", "audio/mpeg" };
            TargetDirectory = "Misc";
        }

        public override FormControl GetControl()
        {
            return new FileUploadFormControl(this);
        }

        public override FormControl GetControl(object obj)
        {
            return new FileUploadFormControl(this, obj);
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
    }
}
