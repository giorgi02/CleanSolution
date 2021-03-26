using Microsoft.AspNetCore.Http;

namespace CleanSolution.Core.Application.Interfaces.Contracts
{
    public interface IFileManager
    {
        string SaveFile(IFormFile file);
    }
}
