using Microsoft.AspNetCore.Http;

namespace $safeprojectname$.Interfaces.Contracts
{
    public interface IFileManager
    {
        string SaveFile(IFormFile file);
    }
}
