using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace $safeprojectname$.Interfaces.Contracts
{
    public interface IFileManager
    {
        public void SaveFile(List<IFormFile> files);
    }
}
