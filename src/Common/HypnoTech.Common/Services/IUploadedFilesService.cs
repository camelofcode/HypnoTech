using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Common.Services
{
    public interface IUploadedFilesService
    {
        FileResult GetImage(string section, string filename);
        FileResult GetFile(string section, string filename);
    }
}
