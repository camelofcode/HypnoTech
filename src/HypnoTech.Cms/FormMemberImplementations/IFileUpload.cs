using System.Collections.Generic;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public interface IFileUpload
    {
        List<string> AllowedFileTypes { get; }

        string TargetDirectory { get; }

        string DisplayText { get; }

        IFileUpload SetAllowedFileTypes(params string[] allowedFileTypes);

        IFileUpload SetTargetDirectory(string targetDirectory);

        FormMemberMapping WithDisplayText(string displayText);
    }
}